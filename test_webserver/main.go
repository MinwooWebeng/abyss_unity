package main

import (
	"io"
	"log"
	"mime"
	"net/http"
	"os"
	"path"
	"strings"

	"golang.org/x/time/rate"
)

type rateLimitedReader struct {
	reader  io.ReadCloser
	limiter *rate.Limiter
}

func (r *rateLimitedReader) Read(p []byte) (int, error) {
	// Wait for a token for each byte to be read
	n := len(p)
	for i := 0; i < n; i++ {
		r.limiter.WaitN(nil, 1)
	}
	return r.reader.Read(p)
}

func (r *rateLimitedReader) Close() error {
	return r.reader.Close()
}

// Middleware to wrap FileServer with bandwidth limiting
func limitFileServer(bytesPerSec int) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		path := "." + r.URL.Path
		f, err := os.Open(path)
		if err != nil {
			http.NotFound(w, r)
			return
		}
		defer f.Close()

		limiter := rate.NewLimiter(rate.Limit(bytesPerSec), bytesPerSec)

		// Copy with throttling
		buf := make([]byte, 32*1024)
		for {
			n, err := f.Read(buf)
			if n > 0 {
				if err := limiter.WaitN(r.Context(), n); err != nil {
					return
				}
				if _, werr := w.Write(buf[:n]); werr != nil {
					return
				}
			}
			if err == io.EOF {
				break
			}
			if err != nil {
				return
			}
		}
	})
}

func main() {
	const addr = "127.0.0.1:8899"

	// Register custom MIME type
	_ = mime.AddExtensionType(".aml", "text/aml")

	// Wrap the file server with custom Content-Type logic
	fs := http.FileServer(http.Dir("."))
	//fs := limitFileServer(100000)

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		// Check if file ends with .aml
		if strings.HasSuffix(r.URL.Path, ".aml") {
			w.Header().Set("Content-Type", "text/aml")
		}
		// Prevent directory traversal by cleaning the path
		r.URL.Path = path.Clean(r.URL.Path)
		fs.ServeHTTP(w, r)
	})

	log.Printf("Serving on http://localhost%s/\n", addr)
	err := http.ListenAndServe(addr, nil)
	if err != nil {
		log.Fatal(err)
	}
}
