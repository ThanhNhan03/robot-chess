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
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://192.168.1.85:7096";

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
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        .container {{
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            padding: 40px;
            max-width: 400px;
            width: 100%;
            text-align: center;
        }}
        .logo {{
            width: 80px;
            height: 80px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 50%;
            margin: 0 auto 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
        }}
        h1 {{
            color: #1a202c;
            font-size: 28px;
            margin-bottom: 10px;
        }}
        p {{
            color: #718096;
            margin-bottom: 30px;
            line-height: 1.6;
        }}
        .google-btn {{
            width: 100%;
            padding: 16px;
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
            transition: all 0.3s;
        }}
        .google-btn:hover {{
            background: #f7fafc;
            border-color: #cbd5e0;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }}
        .google-btn:active {{
            transform: translateY(0);
        }}
        .google-btn:disabled {{
            opacity: 0.6;
            cursor: not-allowed;
        }}
        .status {{
            margin-top: 20px;
            padding: 12px;
            border-radius: 8px;
            font-size: 14px;
            display: none;
        }}
        .status.loading {{
            background: #ebf8ff;
            color: #2c5282;
            display: block;
        }}
        .status.success {{
            background: #c6f6d5;
            color: #22543d;
            display: block;
        }}
        .status.error {{
            background: #fed7d7;
            color: #742a2a;
            display: block;
        }}
        .spinner {{
            width: 20px;
            height: 20px;
            border: 3px solid #f3f3f3;
            border-top: 3px solid #667eea;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            display: inline-block;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>♟️</div>
        <h1>Welcome to Robot Chess</h1>
        <p>Click the button below to sign in with your Google account</p>
        
        <button id='googleBtn' class='google-btn'>
            <svg width='20' height='20' viewBox='0 0 24 24'>
                <path fill='#DB4437' d='M12.545,10.239v3.821h5.445c-0.712,2.315-2.647,3.972-5.445,3.972c-3.332,0-6.033-2.701-6.033-6.032s2.701-6.032,6.033-6.032c1.498,0,2.866,0.549,3.921,1.453l2.814-2.814C17.503,2.988,15.139,2,12.545,2C7.021,2,2.543,6.477,2.543,12s4.478,10,10.002,10c8.396,0,10.249-7.85,9.426-11.748L12.545,10.239z'/>
            </svg>
            Continue with Google
        </button>

        <div id='status' class='status'></div>
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
            statusDiv.textContent = message;
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
