using Microsoft.AspNetCore.Mvc;

namespace robot_chess_api.Controllers;

[ApiController]
[Route("auth")]
public class AuthCallbackController : ControllerBase
{
    private readonly ILogger<AuthCallbackController> _logger;
    private readonly IConfiguration _configuration;

    public AuthCallbackController(ILogger<AuthCallbackController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Serve HTML page for QR code callback with Google OAuth button
    /// </summary>
    [HttpGet("qr-callback")]
    public IActionResult QRCallback([FromQuery] string session)
    {
        if (string.IsNullOrEmpty(session))
        {
            return BadRequest("Session ID is required");
        }

        _logger.LogInformation($"QR callback page requested for session: {session}");

        var supabaseUrl = _configuration["Supabase:Url"];
        var supabaseAnonKey = _configuration["Supabase:AnonKey"];
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://100.73.130.46:7096";

        var html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Robot Chess - Login</title>
    <script src='https://cdn.jsdelivr.net/npm/@supabase/supabase-js@2'></script>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #1567b1 0%, #0d4a7a 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        .container {{
            background: white;
            border-radius: 24px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            padding: 48px 40px;
            max-width: 440px;
            width: 100%;
            text-align: center;
        }}
        .logo-container {{
            width: 100px;
            height: 100px;
            background: linear-gradient(135deg, #f16f23 0%, #ff8c42 100%);
            border-radius: 50%;
            margin: 0 auto 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 48px;
            box-shadow: 0 8px 24px rgba(241, 111, 35, 0.3);
        }}
        h1 {{
            color: #1a202c;
            font-size: 32px;
            font-weight: 700;
            margin-bottom: 12px;
        }}
        .subtitle {{
            color: #718096;
            font-size: 16px;
            margin-bottom: 36px;
            line-height: 1.6;
        }}
        .divider {{
            display: flex;
            align-items: center;
            margin: 24px 0;
            color: #a0aec0;
            font-size: 14px;
        }}
        .divider::before,
        .divider::after {{
            content: '';
            flex: 1;
            height: 1px;
            background: #e2e8f0;
        }}
        .divider::before {{
            margin-right: 12px;
        }}
        .divider::after {{
            margin-left: 12px;
        }}
        .google-btn {{
            width: 100%;
            padding: 16px 24px;
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 600;
            color: #1a202c;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }}
        .google-btn:hover {{
            background: #f7fafc;
            border-color: #cbd5e0;
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(0,0,0,0.1);
        }}
        .google-btn:active {{
            transform: translateY(0);
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }}
        .google-btn:disabled {{
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }}
        .status {{
            margin-top: 24px;
            padding: 14px 18px;
            border-radius: 12px;
            font-size: 14px;
            font-weight: 500;
            display: none;
            animation: slideIn 0.3s ease;
        }}
        .status.loading {{
            background: rgba(21, 103, 177, 0.1);
            color: #1567b1;
            border: 1px solid rgba(21, 103, 177, 0.2);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }}
        .status.success {{
            background: rgba(35, 178, 73, 0.1);
            color: #23b249;
            border: 1px solid rgba(35, 178, 73, 0.2);
            display: block;
        }}
        .status.error {{
            background: rgba(239, 68, 68, 0.1);
            color: #dc2626;
            border: 1px solid rgba(239, 68, 68, 0.2);
            display: block;
        }}
        .spinner {{
            width: 18px;
            height: 18px;
            border: 3px solid rgba(21, 103, 177, 0.2);
            border-top: 3px solid #1567b1;
            border-radius: 50%;
            animation: spin 0.8s linear infinite;
            display: inline-block;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        @keyframes slideIn {{
            from {{
                opacity: 0;
                transform: translateY(-10px);
            }}
            to {{
                opacity: 1;
                transform: translateY(0);
            }}
        }}
        .footer {{
            margin-top: 32px;
            padding-top: 24px;
            border-top: 1px solid #e2e8f0;
            color: #a0aec0;
            font-size: 13px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo-container'>🎮</div>
        <h1>Welcome Back!</h1>
        <p class='subtitle'>Sign in to continue your chess journey</p>
        
        <div class='divider'>Or continue with</div>
        
        <button id='googleBtn' class='google-btn'>
            <svg width='20' height='20' viewBox='0 0 24 24'>
                <path fill='#4285F4' d='M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z'/>
                <path fill='#34A853' d='M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z'/>
                <path fill='#FBBC05' d='M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z'/>
                <path fill='#EA4335' d='M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z'/>
            </svg>
            Continue with Google
        </button>

        <div id='status' class='status'></div>
        
        <div class='footer'>
            Secure authentication powered by Google
        </div>
    </div>

    <script>
        const SESSION_ID = '{session}';
        const SUPABASE_URL = '{supabaseUrl}';
        const SUPABASE_ANON_KEY = '{supabaseAnonKey}';
        const API_BASE_URL = '{apiBaseUrl}';

        const {{ createClient }} = supabase;
        const supabaseClient = createClient(SUPABASE_URL, SUPABASE_ANON_KEY);

        const googleBtn = document.getElementById('googleBtn');
        const statusDiv = document.getElementById('status');

        function showStatus(message, type) {{
            if (type === 'loading') {{
                statusDiv.innerHTML = '<div class=""spinner""></div>' + message;
            }} else {{
                statusDiv.textContent = message;
            }}
            statusDiv.className = 'status ' + type;
        }}

        async function handleGoogleLogin() {{
            try {{
                googleBtn.disabled = true;
                showStatus('Opening Google sign-in...', 'loading');

                const {{ data, error }} = await supabaseClient.auth.signInWithOAuth({{
                    provider: 'google',
                    options: {{
                        redirectTo: window.location.origin + '/auth/callback?session=' + SESSION_ID,
                        queryParams: {{
                            access_type: 'offline',
                            prompt: 'consent',
                        }}
                    }}
                }});

                if (error) {{
                    throw error;
                }}
            }} catch (error) {{
                console.error('Google login error:', error);
                showStatus('Failed to start Google login: ' + error.message, 'error');
                googleBtn.disabled = false;
            }}
        }}

        // Handle callback from Google OAuth
        async function handleCallback() {{
            const hashParams = new URLSearchParams(window.location.hash.substring(1));
            const accessToken = hashParams.get('access_token');

            if (accessToken) {{
                try {{
                    showStatus('Completing sign-in...', 'loading');
                    
                    // Get user info from Supabase
                    const {{ data: {{ user }}, error: userError }} = await supabaseClient.auth.getUser(accessToken);
                    
                    if (userError) throw userError;

                    // Send to backend
                    const response = await fetch(`${{API_BASE_URL}}/api/auth/qr-login`, {{
                        method: 'POST',
                        headers: {{
                            'Content-Type': 'application/json',
                        }},
                        body: JSON.stringify({{
                            sessionId: SESSION_ID,
                            accessToken: accessToken,
                            fullName: user.user_metadata?.full_name,
                            avatarUrl: user.user_metadata?.avatar_url,
                        }})
                    }});

                    const result = await response.json();

                    if (result.success) {{
                        showStatus('✓ Login successful! You can close this window now.', 'success');
                        
                        // Optional: Auto close after 2 seconds
                        setTimeout(() => {{
                            window.close();
                        }}, 2000);
                    }} else {{
                        throw new Error(result.error || 'Login failed');
                    }}
                }} catch (error) {{
                    console.error('Callback error:', error);
                    showStatus('Login failed: ' + error.message, 'error');
                }}
            }}
        }}

        // Check if this is a callback
        if (window.location.hash.includes('access_token')) {{
            handleCallback();
        }} else {{
            googleBtn.addEventListener('click', handleGoogleLogin);
        }}
    </script>
</body>
</html>
";

        return Content(html, "text/html");
    }

    /// <summary>
    /// Handle OAuth callback redirect
    /// </summary>
    [HttpGet("callback")]
    public IActionResult Callback([FromQuery] string session)
    {
        // This will be handled by the JavaScript in the HTML page
        // Just redirect back to the same page with hash params preserved
        return Redirect($"/auth/qr-callback?session={session}");
    }
}
