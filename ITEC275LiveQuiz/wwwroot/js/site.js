// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-refresh functionality for live pages without full page reload
function enableAutoRefresh(intervalMs = 4000) {
    setInterval(() => {
        location.reload();
    }, intervalMs);
}

// Smart polling that only refreshes specific content
function enableSmartPolling(url, targetSelector, intervalMs = 4000) {
    async function poll() {
        try {
            const response = await fetch(url);
            if (response.ok) {
                const html = await response.text();
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newContent = doc.querySelector(targetSelector);
                const currentContent = document.querySelector(targetSelector);
                
                if (newContent && currentContent && newContent.innerHTML !== currentContent.innerHTML) {
                    currentContent.innerHTML = newContent.innerHTML;
                }
            }
        } catch (error) {
            console.error('Polling error:', error);
        }
    }
    
    setInterval(poll, intervalMs);
}

// Debounce function for performance
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
