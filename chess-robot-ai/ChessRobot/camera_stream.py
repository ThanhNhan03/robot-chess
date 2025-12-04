"""
HTTP/MJPEG Camera Streaming Server
Streams raw camera feed with low resolution for smooth performance
"""
import cv2
import threading
from http.server import BaseHTTPRequestHandler, HTTPServer
from socketserver import ThreadingMixIn
import time

class CameraStream:
    def __init__(self, camera_index=0, width=640, height=480, fps=30):
        """
        Initialize camera stream
        
        Args:
            camera_index: Camera device index (default: 0)
            width: Frame width (default: 640 for low resolution)
            height: Frame height (default: 480 for low resolution)
            fps: Target frames per second (default: 30)
        """
        self.camera_index = camera_index
        self.width = width
        self.height = height
        self.fps = fps
        
        self.frame = None
        self.lock = threading.Lock()
        self.running = False
        self.capture_thread = None
        
    def start_capture(self, cam=None):
        """Start capturing frames from camera"""
        if cam is None:
            self.cam = cv2.VideoCapture(self.camera_index)
            self.cam.set(cv2.CAP_PROP_FRAME_WIDTH, self.width)
            self.cam.set(cv2.CAP_PROP_FRAME_HEIGHT, self.height)
            self.cam.set(cv2.CAP_PROP_FPS, self.fps)
        else:
            self.cam = cam
            
        self.running = True
        self.capture_thread = threading.Thread(target=self._capture_loop, daemon=True)
        self.capture_thread.start()
        print(f"[CAMERA STREAM] Started capturing at {self.width}x{self.height} @ {self.fps}fps")
        
    def _capture_loop(self):
        """Internal loop to capture frames"""
        while self.running:
            ret, frame = self.cam.read()
            if ret:
                # Resize to low resolution for smooth streaming
                frame_resized = cv2.resize(frame, (self.width, self.height))
                
                with self.lock:
                    self.frame = frame_resized.copy()
            time.sleep(1.0 / self.fps)
            
    def get_frame(self):
        """Get current frame as JPEG bytes"""
        with self.lock:
            if self.frame is None:
                return None
            
            # Encode frame as JPEG with quality for smooth streaming
            ret, jpeg = cv2.imencode('.jpg', self.frame, [cv2.IMWRITE_JPEG_QUALITY, 70])
            if ret:
                return jpeg.tobytes()
        return None
        
    def stop(self):
        """Stop camera capture"""
        self.running = False
        if self.capture_thread:
            self.capture_thread.join()
        print("[CAMERA STREAM] Stopped")


class StreamingHandler(BaseHTTPRequestHandler):
    """HTTP handler for MJPEG streaming"""
    
    camera_stream = None
    
    def _set_cors_headers(self):
        """Set CORS headers to allow cross-origin requests"""
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, HEAD, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.send_header('Cross-Origin-Resource-Policy', 'cross-origin')
        self.send_header('Cross-Origin-Embedder-Policy', 'unsafe-none')
    
    def do_OPTIONS(self):
        """Handle OPTIONS requests for CORS preflight"""
        self.send_response(200)
        self._set_cors_headers()
        self.end_headers()
    
    def do_HEAD(self):
        """Handle HEAD requests for connection testing"""
        self.send_response(200)
        self.send_header('Content-type', 'multipart/x-mixed-replace; boundary=--jpgboundary')
        self._set_cors_headers()
        self.end_headers()
    
    def do_GET(self):
        """Handle GET requests"""
        if self.path == '/' or self.path == '/stream':
            # Both / and /stream return MJPEG stream directly
            self.send_response(200)
            self.send_header('Content-type', 'multipart/x-mixed-replace; boundary=--jpgboundary')
            self._set_cors_headers()
            self.end_headers()
            
            try:
                while True:
                    if self.camera_stream:
                        frame_bytes = self.camera_stream.get_frame()
                        if frame_bytes:
                            self.wfile.write(b"--jpgboundary\r\n")
                            self.wfile.write(b"Content-type: image/jpeg\r\n")
                            self.wfile.write(f"Content-length: {len(frame_bytes)}\r\n".encode())
                            self.wfile.write(b"\r\n")
                            self.wfile.write(frame_bytes)
                            self.wfile.write(b"\r\n")
                    time.sleep(0.033)  # ~30fps
            except BrokenPipeError:
                print("[CAMERA STREAM] Client disconnected")
            except Exception as e:
                print(f"[CAMERA STREAM] Error: {e}")
        else:
            self.send_response(404)
            self.end_headers()
            
    def log_message(self, format, *args):
        """Suppress default logging"""
        pass


class ThreadedHTTPServer(ThreadingMixIn, HTTPServer):
    """Handle requests in separate threads"""
    pass


class MJPEGStreamServer:
    """MJPEG streaming server"""
    
    def __init__(self, camera_stream, host='0.0.0.0', port=8000):
        """
        Initialize MJPEG server
        
        Args:
            camera_stream: CameraStream instance
            host: Server host (default: 0.0.0.0 - all interfaces)
            port: Server port (default: 8000)
        """
        self.camera_stream = camera_stream
        self.host = host
        self.port = port
        self.server = None
        self.server_thread = None
        
    def start(self):
        """Start the streaming server"""
        StreamingHandler.camera_stream = self.camera_stream
        
        self.server = ThreadedHTTPServer((self.host, self.port), StreamingHandler)
        self.server_thread = threading.Thread(target=self.server.serve_forever, daemon=True)
        self.server_thread.start()
        
        print(f"[MJPEG SERVER] Started on http://{self.host}:{self.port}")
        print(f"[MJPEG SERVER] View stream at: http://localhost:{self.port}")
        
    def stop(self):
        """Stop the streaming server"""
        if self.server:
            self.server.shutdown()
            self.server.server_close()
        print("[MJPEG SERVER] Stopped")


# Example usage
if __name__ == "__main__":
    # Create camera stream with low resolution for smooth streaming
    camera_stream = CameraStream(camera_index=0, width=640, height=480, fps=30)
    camera_stream.start_capture()
    
    # Start MJPEG server
    server = MJPEGStreamServer(camera_stream, host='0.0.0.0', port=8000)
    server.start()
    
    print("\nPress Ctrl+C to stop...")
    
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nStopping...")
        server.stop()
        camera_stream.stop()
