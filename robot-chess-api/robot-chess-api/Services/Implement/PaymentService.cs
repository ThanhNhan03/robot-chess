using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

public class PaymentService : IPaymentService
{
    private readonly PostgresContext _context;
    private readonly IPointPackageRepository _packageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly HttpClient _httpClient;

    // PayOS Configuration
    private readonly string _payOsClientId;
    private readonly string _payOsApiKey;
    private readonly string _payOsChecksumKey;
    private readonly string _payOsBaseUrl;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;

    public PaymentService(
        PostgresContext context,
        IPointPackageRepository packageRepository,
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<PaymentService> logger,
        HttpClient httpClient)
    {
        _context = context;
        _packageRepository = packageRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        // Load PayOS configuration
        _payOsClientId = _configuration["PayOS:ClientId"] ?? throw new InvalidOperationException("PayOS ClientId not configured");
        _payOsApiKey = _configuration["PayOS:ApiKey"] ?? throw new InvalidOperationException("PayOS ApiKey not configured");
        _payOsChecksumKey = _configuration["PayOS:ChecksumKey"] ?? throw new InvalidOperationException("PayOS ChecksumKey not configured");
        _payOsBaseUrl = _configuration["PayOS:BaseUrl"] ?? "https://api-merchant.payos.vn";
        _returnUrl = _configuration["PayOS:ReturnUrl"] ?? "http://localhost:5173/payment/success";
        _cancelUrl = _configuration["PayOS:CancelUrl"] ?? "http://localhost:5173/payment/cancel";
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(Guid userId, int packageId)
    {
        try
        {
            // Validate user exists
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Get package
            var package = await _packageRepository.GetByIdAsync(packageId);
            _logger.LogInformation("Looking for package ID: {PackageId}, Found: {Found}, IsActive: {IsActive}", 
                packageId, package != null, package?.IsActive);
            
            if (package == null || !package.IsActive)
            {
                throw new Exception("Package not found or inactive");
            }

            // Create order code (unique transaction ID)
            var orderCode = $"{DateTime.UtcNow:yyyyMMddHHmmss}{userId.ToString().Substring(0, 8)}";

            // Prepare PayOS request - use long for orderCode
            long orderCodeLong;
            if (!long.TryParse(orderCode.Substring(0, Math.Min(14, orderCode.Length)), out orderCodeLong))
            {
                orderCodeLong = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            // Create payment history record
            var paymentHistory = new PaymentHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = packageId,
                TransactionId = orderCode,
                OrderCode = orderCodeLong.ToString(),  // Save PayOS order code
                Amount = package.Price,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentHistories.Add(paymentHistory);
            await _context.SaveChangesAsync();

            var paymentRequest = new
            {
                orderCode = orderCodeLong,
                amount = (int)package.Price,
                description = $"Mua {package.Name} - {package.Points} diem",
                cancelUrl = _cancelUrl,
                returnUrl = _returnUrl
            };

            // Generate signature - only use amount, cancelUrl, description, orderCode, returnUrl
            var signatureData = new
            {
                amount = paymentRequest.amount,
                cancelUrl = paymentRequest.cancelUrl,
                description = paymentRequest.description,
                orderCode = paymentRequest.orderCode,
                returnUrl = paymentRequest.returnUrl
            };
            
            var signature = GenerateSignature(signatureData);

            // Create final request with signature
            var paymentRequestWithSignature = new
            {
                orderCode = orderCodeLong,
                amount = (int)package.Price,
                description = paymentRequest.description,
                cancelUrl = _cancelUrl,
                returnUrl = _returnUrl,
                signature = signature
            };

            _logger.LogInformation("PayOS Request: {@Request}", paymentRequestWithSignature);

            // Call PayOS API
            var jsonContent = JsonSerializer.Serialize(paymentRequestWithSignature, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_payOsBaseUrl}/v2/payment-requests")
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-client-id", _payOsClientId);
            request.Headers.Add("x-api-key", _payOsApiKey);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("PayOS Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("PayOS Response Content: {Content}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"PayOS API error: {responseContent}");
                throw new Exception("Failed to create payment link");
            }

            var payOsResponse = JsonSerializer.Deserialize<PayOsCreatePaymentResponse>(responseContent);
            
            _logger.LogInformation("PayOS Parsed Response - Code: {Code}, Desc: {Desc}, Data: {Data}", 
                payOsResponse?.code, payOsResponse?.desc, 
                payOsResponse?.data != null ? "Present" : "Null");
            
            if (payOsResponse?.data != null)
            {
                _logger.LogInformation("PayOS Data - CheckoutUrl: {CheckoutUrl}, QrCode: {QrCode}", 
                    payOsResponse.data.checkoutUrl, payOsResponse.data.qrCode);
            }

            return new PaymentResponseDto
            {
                PaymentUrl = payOsResponse?.data?.checkoutUrl ?? "",
                TransactionId = orderCode,
                Amount = package.Price,
                QrCodeUrl = payOsResponse?.data?.qrCode ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            throw;
        }
    }

    public async Task<PaymentStatusDto> CheckPaymentStatusAsync(string orderCode)
    {
        // Try to find by OrderCode first, then by TransactionId
        var payment = await _context.PaymentHistories
            .Include(p => p.Package)
            .FirstOrDefaultAsync(p => p.OrderCode == orderCode || p.TransactionId == orderCode);

        if (payment == null)
        {
            throw new Exception("Payment not found");
        }

        // If payment is still pending, check with PayOS
        if (payment.Status == "pending")
        {
            try
            {
                // Call PayOS API to get payment info - use OrderCode (numeric) not TransactionId
                var payOsOrderCode = payment.OrderCode ?? orderCode;
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_payOsBaseUrl}/v2/payment-requests/{payOsOrderCode}");
                request.Headers.Add("x-client-id", _payOsClientId);
                request.Headers.Add("x-api-key", _payOsApiKey);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("PayOS Get Payment Response for OrderCode {OrderCode}: {Content}", payOsOrderCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var payOsResponse = JsonSerializer.Deserialize<PayOsCreatePaymentResponse>(responseContent);
                    
                    // Check if payment is completed
                    if (payOsResponse?.data?.status == "PAID")
                    {
                        // Update payment status
                        payment.Status = "success";
                        await _context.SaveChangesAsync();

                        // Credit points to user
                        if (payment.UserId.HasValue)
                        {
                            var user = await _userRepository.GetUserByIdAsync(payment.UserId.Value);
                            if (user != null && payment.Package != null)
                            {
                                user.PointsBalance += payment.Package.Points;
                            await _userRepository.UpdateUserAsync(user);

                            // Create point transaction record
                            var pointTransaction = new PointTransaction
                            {
                                Id = Guid.NewGuid(),
                                UserId = payment.UserId.Value,
                                Amount = payment.Package.Points,
                                TransactionType = "deposit",
                                Description = $"Mua {payment.Package.Name}",
                                RelatedPaymentId = payment.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.PointTransactions.Add(pointTransaction);
                            await _context.SaveChangesAsync();

                            _logger.LogInformation("Points credited for payment {OrderCode}: {Points} points", 
                                orderCode, payment.Package.Points);
                            }
                        }
                    }
                    else if (payOsResponse?.data?.status == "CANCELLED")
                    {
                        payment.Status = "failed";
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking PayOS payment status for {OrderCode}", orderCode);
            }
        }

        return new PaymentStatusDto
        {
            TransactionId = payment.TransactionId!,
            Status = payment.Status!,
            Amount = payment.Amount,
            CompletedAt = payment.Status == "success" ? payment.CreatedAt : null
        };
    }

    public async Task<bool> ProcessWebhookAsync(PaymentWebhookDto webhookData)
    {
        try
        {
            if (!webhookData.Success || webhookData.Data == null)
            {
                return false;
            }

            var transactionId = webhookData.Data.OrderCode;
            var payment = await _context.PaymentHistories
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

            if (payment == null)
            {
                _logger.LogWarning($"Payment not found for transaction: {transactionId}");
                return false;
            }

            // Update payment status
            payment.Status = "success";
            
            // Get package to know how many points to add
            var package = await _packageRepository.GetByIdAsync(payment.PackageId!.Value);
            if (package == null)
            {
                _logger.LogError($"Package not found: {payment.PackageId}");
                return false;
            }

            // Create point transaction
            var pointTransaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                UserId = payment.UserId!.Value,
                Amount = package.Points,
                TransactionType = "deposit",
                Description = $"Mua {package.Name}",
                RelatedPaymentId = payment.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.PointTransactions.Add(pointTransaction);

            // Update user points balance
            var user = payment.User;
            if (user != null)
            {
                user.PointsBalance += package.Points;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Payment processed successfully for user {payment.UserId}, added {package.Points} points");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment webhook");
            return false;
        }
    }

    private string GenerateSignature(object data)
    {
        // Convert object to dictionary
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        if (dict == null) return "";

        // Sort by key alphabetically and build query string
        var sortedKeys = dict.Keys.OrderBy(k => k).ToList();
        var queryString = string.Join("&", sortedKeys.Select(key =>
        {
            var value = dict[key];
            var stringValue = value.ValueKind == JsonValueKind.String 
                ? value.GetString() 
                : value.ToString();
            return $"{key}={stringValue}";
        }));

        _logger.LogInformation("Signature data string: {Data}", queryString);

        // Generate HMAC SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_payOsChecksumKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
        var signature = Convert.ToHexString(hashBytes).ToLower();
        
        _logger.LogInformation("Generated signature: {Signature}", signature);
        
        return signature;
    }

    public async Task<IEnumerable<PaymentHistory>> GetAllPaymentsAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        string? status = null)
    {
        var paymentRepo = new PaymentHistoryRepository(_context);
        return await paymentRepo.GetAllAsync(startDate, endDate, status);
    }

    public async Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var paymentRepo = new PaymentHistoryRepository(_context);
        return await paymentRepo.GetStatisticsAsync(startDate, endDate);
    }

    public async Task<IEnumerable<PaymentHistory>> GetUserPaymentHistoryAsync(Guid userId)
    {
        var paymentRepo = new PaymentHistoryRepository(_context);
        return await paymentRepo.GetByUserIdAsync(userId);
    }
}

// PayOS Response Models
public class PayOsCreatePaymentResponse
{
    public string? code { get; set; }
    public string? desc { get; set; }
    public PayOsPaymentData? data { get; set; }
}

public class PayOsPaymentData
{
    public string? bin { get; set; }
    public string? accountNumber { get; set; }
    public string? accountName { get; set; }
    public int? amount { get; set; }
    public string? description { get; set; }
    public long? orderCode { get; set; }  // Changed from int to long
    public string? currency { get; set; }
    public string? paymentLinkId { get; set; }
    public string? status { get; set; }
    public string? checkoutUrl { get; set; }
    public string? qrCode { get; set; }
}
