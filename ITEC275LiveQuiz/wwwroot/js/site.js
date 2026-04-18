// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Add smooth page transitions
document.addEventListener('DOMContentLoaded', function () {
    // Fade in on load
    document.body.style.opacity = '0';
    setTimeout(() => {
        document.body.style.transition = 'opacity 0.3s';
        document.body.style.opacity = '1';
    }, 10);

    // Add loading indicator to forms
    const forms = document.querySelectorAll('form[method="post"]');
    forms.forEach(form => {
        form.addEventListener('submit', function () {
            const btn = form.querySelector('button[type="submit"]');
            if (btn && !btn.disabled) {
                btn.disabled = true;
                const originalText = btn.innerHTML;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
                setTimeout(() => {
                    btn.innerHTML = originalText;
                    btn.disabled = false;
                }, 3000);
            }
        });
    });

    // Confetti effect for winners
    if (window.location.pathname.includes('Stats') || window.location.pathname.includes('EndGame')) {
        const firstPlace = document.querySelector('.rank-1');
        if (firstPlace) {
            setTimeout(launchConfetti, 500);
        }
    }
});

// Confetti animation
function launchConfetti() {
    const colors = ['#ffd700', '#46178f', '#26890d', '#e21b3c', '#1368ce'];
    const confettiCount = 50;

    for (let i = 0; i < confettiCount; i++) {
        const confetti = document.createElement('div');
        confetti.style.position = 'fixed';
        confetti.style.width = '10px';
        confetti.style.height = '10px';
        confetti.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
        confetti.style.left = Math.random() * 100 + '%';
        confetti.style.top = '-10px';
        confetti.style.borderRadius = '50%';
        confetti.style.pointerEvents = 'none';
        confetti.style.zIndex = '9999';
        confetti.style.animation = `fall ${2 + Math.random() * 2}s linear forwards`;

        document.body.appendChild(confetti);
        setTimeout(() => confetti.remove(), 4000);
    }
}

// Add confetti CSS animation
const style = document.createElement('style');
style.textContent = `
    @keyframes fall {
        to {
            transform: translateY(100vh) rotate(360deg);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);
