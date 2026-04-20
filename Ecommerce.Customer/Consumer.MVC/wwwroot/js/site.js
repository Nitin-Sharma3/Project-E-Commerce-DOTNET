// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/* ─── TechNova Main JS ─── */
(function () {
    'use strict';

    /* ── CURSOR ─────────────────────────────────────────────────────────────── */
    const dot = document.getElementById('cursor-dot');
    const ring = document.getElementById('cursor-ring');
    const glow = document.getElementById('cursor-glow');

    let mx = 0, my = 0;
    let rx = 0, ry = 0;
    let gx = 0, gy = 0;

    document.addEventListener('mousemove', e => { mx = e.clientX; my = e.clientY; });

    function animCursor() {
        if (dot) {
            dot.style.left = mx + 'px';
            dot.style.top = my + 'px';
        }
        if (ring) {
            rx += (mx - rx) * 0.14;
            ry += (my - ry) * 0.14;
            ring.style.left = rx + 'px';
            ring.style.top = ry + 'px';
        }
        if (glow) {
            gx += (mx - gx) * 0.06;
            gy += (my - gy) * 0.06;
            glow.style.left = gx + 'px';
            glow.style.top = gy + 'px';
        }
        requestAnimationFrame(animCursor);
    }
    animCursor();

    /* Magnetic attraction */
    document.querySelectorAll('[data-magnetic]').forEach(el => {
        el.addEventListener('mouseenter', () => document.body.classList.add('cursor-magnetic'));
        el.addEventListener('mouseleave', () => document.body.classList.remove('cursor-magnetic'));
        el.addEventListener('mousemove', e => {
            const r = el.getBoundingClientRect();
            const cx = r.left + r.width / 2;
            const cy = r.top + r.height / 2;
            const dx = (e.clientX - cx) * 0.25;
            const dy = (e.clientY - cy) * 0.25;
            el.style.transform = `translate(${dx}px, ${dy}px)`;
        });
        el.addEventListener('mouseleave', () => { el.style.transform = ''; });
    });

    /* Hover state */
    document.querySelectorAll('a, button, [data-hover], .product-card, .category-item, .btn-icon').forEach(el => {
        el.addEventListener('mouseenter', () => document.body.classList.add('cursor-hover'));
        el.addEventListener('mouseleave', () => document.body.classList.remove('cursor-hover'));
    });

    /* ── NAVBAR SCROLL ───────────────────────────────────────────────────────── */
    const navbar = document.querySelector('.navbar');
    window.addEventListener('scroll', () => {
        navbar && navbar.classList.toggle('scrolled', window.scrollY > 40);
    }, { passive: true });

    /* ── HERO PARTICLES CANVAS ───────────────────────────────────────────────── */
    const canvas = document.getElementById('hero-canvas');
    if (canvas) {
        const ctx = canvas.getContext('2d');
        let particles = [];
        let W, H;

        function resize() {
            W = canvas.width = canvas.offsetWidth;
            H = canvas.height = canvas.offsetHeight;
        }
        resize();
        window.addEventListener('resize', resize, { passive: true });

        const COLORS = ['rgba(59,130,246,', 'rgba(139,92,246,', 'rgba(6,182,212,'];

        class Particle {
            constructor() { this.reset(); }
            reset() {
                this.x = Math.random() * W;
                this.y = Math.random() * H;
                this.size = Math.random() * 1.8 + 0.4;
                this.color = COLORS[Math.floor(Math.random() * COLORS.length)];
                this.alpha = Math.random() * 0.5 + 0.1;
                this.vx = (Math.random() - 0.5) * 0.25;
                this.vy = (Math.random() - 0.5) * 0.25 - 0.1;
                this.life = 1;
                this.decay = Math.random() * 0.002 + 0.001;
            }
            draw() {
                ctx.save();
                ctx.globalAlpha = this.alpha * this.life;
                ctx.fillStyle = this.color + this.alpha + ')';
                ctx.shadowColor = this.color + '0.8)';
                ctx.shadowBlur = 6;
                ctx.beginPath();
                ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
                ctx.fill();
                ctx.restore();
            }
            update() {
                this.x += this.vx;
                this.y += this.vy;
                this.life -= this.decay;
                if (this.life <= 0 || this.x < 0 || this.x > W || this.y < 0 || this.y > H) this.reset();
                this.draw();
            }
        }

        for (let i = 0; i < 80; i++) particles.push(new Particle());

        function animParticles() {
            ctx.clearRect(0, 0, W, H);
            particles.forEach(p => p.update());
            requestAnimationFrame(animParticles);
        }
        animParticles();

        /* mouse-reactive burst */
        canvas.parentElement.addEventListener('mousemove', e => {
            const r = canvas.getBoundingClientRect();
            const px = e.clientX - r.left;
            const py = e.clientY - r.top;
            if (Math.random() > 0.82) {
                const p = new Particle();
                p.x = px; p.y = py;
                p.vx = (Math.random() - 0.5) * 1.5;
                p.vy = (Math.random() - 0.5) * 1.5 - 0.5;
                p.size = Math.random() * 2.5 + 0.8;
                p.life = 1.2;
                particles.push(p);
                if (particles.length > 150) particles.shift();
            }
        });
    }

    /* ── LAPTOP OPEN ON HOVER ────────────────────────────────────────────────── */
    const laptopWrap = document.querySelector('.laptop-wrap');
    const laptop = document.querySelector('.laptop');
    if (laptopWrap && laptop) {
        laptopWrap.addEventListener('mouseenter', () => laptop.classList.add('open'));
        laptopWrap.addEventListener('mouseleave', () => laptop.classList.remove('open'));
    }

    /* ── PRODUCT CARD 3D TILT ────────────────────────────────────────────────── */
    document.querySelectorAll('.product-card').forEach(card => {
        card.addEventListener('mousemove', e => {
            const r = card.getBoundingClientRect();
            const cx = r.left + r.width / 2;
            const cy = r.top + r.height / 2;
            const dx = (e.clientX - cx) / (r.width / 2);
            const dy = (e.clientY - cy) / (r.height / 2);
            const rotY = dx * 12;
            const rotX = -dy * 8;
            card.style.transform = `perspective(900px) rotateY(${rotY}deg) rotateX(${rotX}deg) scale(1.02)`;
            card.style.boxShadow = `${-dx * 12}px ${-dy * 8}px 40px rgba(0,0,0,0.4), 0 0 50px rgba(59,130,246,0.1)`;
            /* dynamic shine */
            const shine = card.querySelector('.card-shine');
            if (shine) {
                shine.style.background = `radial-gradient(ellipse at ${(dx + 1) * 50}% ${(dy + 1) * 50}%, rgba(255,255,255,0.06) 0%, transparent 60%)`;
            }
        });
        card.addEventListener('mouseleave', () => {
            card.style.transform = '';
            card.style.boxShadow = '';
        });
    });

    /* ── RIPPLE ON CLICK ─────────────────────────────────────────────────────── */
    document.querySelectorAll('.btn-primary, .btn-cart, .btn-secondary').forEach(btn => {
        btn.addEventListener('click', e => {
            const container = document.createElement('span');
            container.className = 'ripple-container';
            const ripple = document.createElement('span');
            ripple.className = 'ripple';
            const r = btn.getBoundingClientRect();
            const size = Math.max(r.width, r.height) * 1.5;
            ripple.style.cssText = `width:${size}px;height:${size}px;left:${e.clientX - r.left}px;top:${e.clientY - r.top}px;`;
            container.appendChild(ripple);
            btn.style.position = 'relative';
            btn.appendChild(container);
            setTimeout(() => container.remove(), 900);
        });
    });

    /* ── ADD TO CART (API call) ─────────────────────────────────────────────── */
    document.querySelectorAll('[data-add-cart]').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.stopPropagation();
            const productId = btn.dataset.addCart;
            const name = btn.closest('.product-card')?.querySelector('.product-name')?.textContent || 'Product';

            /* Animate button */
            const orig = btn.innerHTML;
            btn.innerHTML = '<i class="fas fa-check"></i> Added';
            btn.style.background = 'linear-gradient(135deg,#10b981,#059669)';
            btn.style.borderColor = 'transparent';
            btn.style.color = 'white';

            try {
                await fetch('/Cart/AddToCart', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiforgery() },
                    body: JSON.stringify({ productId, quantity: 1 })
                });
                updateCartBadge();
            } catch (_) { /* fail silently */ }

            showToast('🛒', `<b>${name}</b> added to cart`);

            setTimeout(() => {
                btn.innerHTML = orig;
                btn.style = '';
            }, 2000);
        });
    });

    /* ── WISHLIST ──────────────────────────────────────────────────────────── */
    document.querySelectorAll('[data-wishlist]').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.stopPropagation();
            const productId = btn.dataset.wishlist;
            btn.classList.toggle('active');
            const adding = btn.classList.contains('active');
            const name = btn.closest('.product-card')?.querySelector('.product-name')?.textContent || 'Product';

            try {
                await fetch(adding ? '/Wishlist/Add' : '/Wishlist/Remove', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiforgery() },
                    body: JSON.stringify({ productId })
                });
            } catch (_) { }

            showToast(adding ? '❤️' : '🤍', adding ? `<b>${name}</b> saved to wishlist` : `Removed from wishlist`);
        });
    });

    /* ── TOAST ───────────────────────────────────────────────────────────────── */
    let toastTimer;
    function showToast(icon, msg) {
        let toast = document.querySelector('.toast');
        if (!toast) {
            toast = document.createElement('div');
            toast.className = 'toast';
            document.body.appendChild(toast);
        }
        toast.innerHTML = `<span class="toast-icon">${icon}</span><span class="toast-msg">${msg}</span>`;
        toast.classList.add('show');
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => toast.classList.remove('show'), 3200);
    }

    /* ── CART BADGE UPDATE ───────────────────────────────────────────────────── */
    async function updateCartBadge() {
        try {
            const r = await fetch('/Cart/Count');
            const d = await r.json();
            const badge = document.querySelector('.cart-badge');
            if (badge && d.count !== undefined) badge.textContent = d.count;
        } catch (_) { }
    }

    function getAntiforgery() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    /* ── SCROLL REVEAL ───────────────────────────────────────────────────────── */
    const revealObs = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                revealObs.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

    document.querySelectorAll('.reveal').forEach(el => revealObs.observe(el));

    /* ── PARALLAX ────────────────────────────────────────────────────────────── */
    window.addEventListener('scroll', () => {
        const scrollY = window.scrollY;
        const heroBg = document.querySelector('.hero-bg');
        if (heroBg) heroBg.style.transform = `translateY(${scrollY * 0.3}px)`;
        const heroContent = document.querySelector('.hero-content');
        if (heroContent) heroContent.style.transform = `translateY(${scrollY * 0.12}px)`;
        const deviceStage = document.querySelector('.device-stage');
        if (deviceStage) deviceStage.style.transform = `translateY(${scrollY * 0.08}px)`;
    }, { passive: true });

    /* ── HERO BG GRADIENT SHIFT ON MOUSE ─────────────────────────────────────── */
    const heroBg = document.querySelector('.hero-bg');
    document.querySelector('.hero')?.addEventListener('mousemove', e => {
        if (!heroBg) return;
        const x = (e.clientX / window.innerWidth) * 100;
        const y = (e.clientY / window.innerHeight) * 100;
        heroBg.style.background = `
      radial-gradient(ellipse 80% 60% at ${x}% ${y - 20}%, rgba(59,130,246,0.1) 0%, transparent 60%),
      radial-gradient(ellipse 60% 50% at ${100 - x}% ${100 - y}%, rgba(139,92,246,0.07) 0%, transparent 55%),
      radial-gradient(ellipse 50% 40% at ${x * 0.6}% ${y * 1.2}%, rgba(6,182,212,0.06) 0%, transparent 50%),
      #050507
    `;
    });

    /* ── CATEGORY COUNT-UP ───────────────────────────────────────────────────── */
    function countUp(el, target, duration = 1500) {
        let start = 0;
        const step = (timestamp) => {
            if (!start) start = timestamp;
            const progress = Math.min((timestamp - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            el.textContent = Math.floor(eased * target).toLocaleString();
            if (progress < 1) requestAnimationFrame(step);
        };
        requestAnimationFrame(step);
    }

    const statObs = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const el = entry.target;
                const val = parseInt(el.dataset.count, 10);
                if (!isNaN(val)) countUp(el, val);
                statObs.unobserve(el);
            }
        });
    }, { threshold: 0.5 });

    document.querySelectorAll('[data-count]').forEach(el => statObs.observe(el));

    /* ── PRODUCT QUICK VIEW SHIMMER ON HOVER ───────────────────────────────── */
    document.querySelectorAll('.product-card').forEach(card => {
        const wrap = card.querySelector('.product-img-wrap');
        if (!wrap) return;
        card.addEventListener('mouseenter', () => {
            wrap.style.boxShadow = '0 0 30px rgba(59,130,246,0.15), inset 0 0 20px rgba(59,130,246,0.05)';
        });
        card.addEventListener('mouseleave', () => { wrap.style.boxShadow = ''; });
    });

    /* ── FEATURED BANNER TILT ────────────────────────────────────────────────── */
    const banner = document.querySelector('.featured-banner');
    if (banner) {
        banner.addEventListener('mousemove', e => {
            const r = banner.getBoundingClientRect();
            const dx = (e.clientX - r.left - r.width / 2) / r.width;
            const dy = (e.clientY - r.top - r.height / 2) / r.height;
            banner.style.transform = `perspective(1200px) rotateY(${dx * 5}deg) rotateX(${-dy * 3}deg)`;
        });
        banner.addEventListener('mouseleave', () => { banner.style.transform = ''; });
    }

})();
// TechNest — site.js

document.addEventListener('DOMContentLoaded', () => {

    // ── Tag Pills ──
    document.querySelectorAll('.tag-pill').forEach(pill => {
        pill.addEventListener('click', () => {
            document.querySelectorAll('.tag-pill').forEach(p => p.classList.remove('active'));
            pill.classList.add('active');
        });
    });

    // ── Add to Cart Feedback ──
    document.querySelectorAll('.add-to-cart-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            const original = btn.textContent;
            btn.textContent = '✓ Added';
            btn.style.background = '#2d3435';
            btn.style.color = '#f9f9f9';
            setTimeout(() => {
                btn.textContent = original;
                btn.style.background = '';
                btn.style.color = '';
            }, 1400);

            // Increment badge
            const badge = document.querySelector('.cart-badge');
            if (badge) badge.textContent = parseInt(badge.textContent || '0') + 1;
        });
    });

    // ── Scroll-triggered fade-in ──
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });

    document.querySelectorAll('.product-card, .testimonial-card, .editorial-banner, .trust-item').forEach(el => {
        el.classList.add('fade-up');
        observer.observe(el);
    });

    // ── Newsletter form ──
    const newsletterBtn = document.querySelector('.newsletter-section .btn-primary');
    const newsletterInput = document.querySelector('.newsletter-input');
    if (newsletterBtn && newsletterInput) {
        newsletterBtn.addEventListener('click', () => {
            if (newsletterInput.value.includes('@')) {
                newsletterBtn.textContent = '✓ Subscribed!';
                newsletterInput.value = '';
                newsletterBtn.disabled = true;
                setTimeout(() => {
                    newsletterBtn.textContent = 'Subscribe';
                    newsletterBtn.disabled = false;
                }, 3000);
            } else {
                newsletterInput.style.background = 'var(--surface-container-highest)';
                newsletterInput.placeholder = 'Please enter a valid email';
            }
        });
    }
});