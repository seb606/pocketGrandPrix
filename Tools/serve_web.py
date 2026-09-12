"""Local Web build server. Usage: python Tools/serve_web.py [port]"""
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import sys
root = Path(__file__).resolve().parents[1] / 'Builds' / 'Web'
if not (root / 'index.html').exists():
    raise SystemExit('Construire le jeu dans Unity : Pocket GP > 3 - Construire pour le Web.')
class Handler(SimpleHTTPRequestHandler):
    extensions_map = {**SimpleHTTPRequestHandler.extensions_map, '.wasm': 'application/wasm', '.data': 'application/octet-stream', '.js': 'application/javascript'}
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(root), **kwargs)
port = int(sys.argv[1]) if len(sys.argv) > 1 else 8080
print(f'Pocket Grand Prix : http://localhost:{port} — Ctrl+C pour arrêter')
ThreadingHTTPServer(('0.0.0.0', port), Handler).serve_forever()
