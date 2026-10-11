import http.server
import os
import socketserver

PORT = 8000


class Handler(http.server.SimpleHTTPRequestHandler):
    def _encoding_for(self, fs_path):
        """Detect how a .unityweb file is compressed from its first bytes."""
        try:
            with open(fs_path, "rb") as f:
                head = f.read(4)
        except OSError:
            return None
        if head[:2] == b"\x1f\x8b":
            return "gzip"
        if head == b"\x00asm":
            return None  # uncompressed wasm
        if fs_path.endswith(".unityweb"):
            return "br"  # otherwise assume Brotli
        return None

    def end_headers(self):
        path = self.path.split("?")[0]
        if path.endswith(".unityweb"):
            enc = self._encoding_for(self.translate_path(path))
            if enc:
                self.send_header("Content-Encoding", enc)
        self.send_header("Cache-Control", "no-store")
        super().end_headers()

    def guess_type(self, path):
        if path.endswith(".wasm.unityweb"):
            return "application/wasm"
        if path.endswith(".js.unityweb"):
            return "application/javascript"
        if path.endswith(".unityweb"):
            return "application/octet-stream"
        return super().guess_type(path)


socketserver.TCPServer.allow_reuse_address = True
with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print(f"Serving {os.getcwd()}")
    print(f"Open http://localhost:{PORT}  (Ctrl+C to stop)")
    httpd.serve_forever()
