(function () {
    // ============================================================
    // ELEMENT REFERENCES
    // ============================================================

    const containerEl = document.querySelector('[data-api-endpoint]');
    const endpoint = containerEl?.dataset.apiEndpoint || '/Akcni/Rezervovat';

    let currentButton = null;
    const dialog = document.getElementById('confirm-dialog');
    const messageEl = document.getElementById('confirm-message');
    const yesBtn = document.getElementById('confirm-yes');
    const cancelBtn = document.getElementById('confirm-cancel');
    const dateInput = document.getElementById('booking-date');
    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : '';

    // ============================================================
    // INITIALIZATION
    // ============================================================

    /**
     * Main initialization function - consolidates all initialization
     */
    function init() {
        // Validate required elements
        if (!dialog || !messageEl) {
            console.error('❌ Confirm dialog elements not found.');
            return;
        }

        console.log('✅ Booking module initialized. API Endpoint:', endpoint);

        // Attach all event listeners
        attachEventListeners();

        // Initialize tooltips
        initializeTooltips();
    }

    /**
     * Attach all event listeners in one place
     */
    function attachEventListeners() {
        attachSeatClickListener();
        attachCancelButtonListener();
        attachConfirmButtonListener();
        attachEscapeKeyListener();
        attachDateChangeListener();
    }

    // ============================================================
    // EVENT LISTENERS
    // ============================================================

    /**
     * Handle seat button clicks - show confirmation dialog
     */
    function attachSeatClickListener() {
        document.addEventListener('click', function (ev) {
            const target = ev.target;
            const btn = (target && typeof target.closest === 'function') ? target.closest('.seat') : null;
            if (!btn) return;

            // Prevent handling clicks from within the confirm dialog
            if (dialog && dialog.contains(btn)) return;

            // Skip if seat is already booked or disabled
            if (btn.classList.contains('booked-other') || btn.disabled) return;

            ev.preventDefault();
            currentButton = btn;
            const seatText = btn.getAttribute('data-seat') || btn.textContent.trim();

            // Get section ID and title from parent overlay
            const overlay = btn.closest('.overlay');
            const sekceDb = overlay ? overlay.getAttribute('data-sekce-db') : '';
            const sectionTitle = overlay ? overlay.getAttribute('data-section-title') : '';

            const selectedDate = dateInput ? dateInput.value : '';

            // Build confirmation message
            messageEl.textContent = (selectedDate)
                ? `Opravdu chcete zabookovat místo ${seatText} (${sectionTitle}) na datum ${selectedDate}?`
                : (sectionTitle ? `Opravdu chcete zabookovat místo ${seatText} (${sectionTitle})?` : `Opravdu chcete zabookovat místo ${seatText}?`);

            // Show dialog
            dialog.setAttribute('aria-hidden', 'false');
            dialog.style.display = 'flex';
        });
    }

    /**
     * Handle cancel button
     */
    function attachCancelButtonListener() {
        if (cancelBtn) {
            cancelBtn.addEventListener('click', function () {
                dialog.setAttribute('aria-hidden', 'true');
                dialog.style.display = 'none';
                currentButton = null;
            });
        }
    }

    /**
     * Handle booking confirmation (YES button)
     */
    function attachConfirmButtonListener() {
        if (yesBtn) {
            yesBtn.addEventListener('click', function () {
                if (!currentButton) {
                    dialog.setAttribute('aria-hidden', 'true');
                    dialog.style.display = 'none';
                    return;
                }

                const btn = currentButton;
                const seatNumber = btn.getAttribute('data-seat') || btn.textContent.trim();
                const overlay = btn.closest('.overlay');
                const sekceDb = overlay ? overlay.getAttribute('data-sekce-db') : (btn.getAttribute('data-sekce-id') || '');
                const selectedDate = dateInput ? dateInput.value : '';

                // Visual feedback: mark as booked
                btn.classList.add('booked-me');
                btn.disabled = true;

                // Build request payload
                const payload = {
                    SekceId: sekceDb || null,
                    SeatNumber: seatNumber,
                    Date: selectedDate || null
                };

                // Send booking request
                const headers = {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                };

                console.log('📡 Sending booking request to:', endpoint);
                fetch(endpoint, {
                    method: 'POST',
                    headers: headers,
                    body: JSON.stringify(payload)
                })
                    .then(function (resp) {
                        console.log('📥 Response status:', resp.status, resp.statusText);
                        if (!resp.ok) {
                            // Rollback visual changes on error
                            btn.classList.remove('booked-me');
                            btn.disabled = false;
                            return resp.text().then(t => {
                                console.error('❌ Server error response:', t);
                                throw new Error(t || resp.statusText);
                            });
                        }
                        return resp.json().catch(() => {
                            console.warn('⚠️ Response is not valid JSON');
                            return null;
                        });
                    })
                    .then(function (data) {
                        console.log('✅ Booking successful!', data);
                        // Success - keep visual changes
                    })
                    .catch(function (err) {
                        console.error('❌ Booking failed:', err);
                        alert('Chyba při ukládání rezervace: ' + (err && err.message ? err.message : 'server error'));
                    })
                    .finally(function () {
                        console.log('🏁 Finally block - closing dialog');
                        dialog.setAttribute('aria-hidden', 'true');
                        dialog.style.display = 'none';
                        currentButton = null;
                    });
            });
        }
    }

    /**
     * Close dialog or overlay on ESC key
     */
    function attachEscapeKeyListener() {
        document.addEventListener('keydown', function (ev) {
            if (ev.key !== 'Escape') return;

            // Priority 1: Close confirmation dialog if open
            if (dialog && dialog.getAttribute('aria-hidden') === 'false') {
                dialog.setAttribute('aria-hidden', 'true');
                dialog.style.display = 'none';
                currentButton = null;
                ev.preventDefault();
                return;
            }

            // Priority 2: Close overlay (section) if open
            const openOverlay = document.querySelector('.overlay:target');
            if (openOverlay) {
                // Navigate to remove the :target state
                window.history.back();
                ev.preventDefault();
                return;
            }
        });
    }

    /**
     * Refresh page when date changes
     */
    function attachDateChangeListener() {
        if (dateInput) {
            dateInput.addEventListener('change', function () {
                const selectedDate = this.value;
                if (selectedDate) {
                    window.location.href = '?bookingDate=' + encodeURIComponent(selectedDate);
                }
            });
        }
    }

    // ============================================================
    // TOOLTIP INITIALIZATION
    // ============================================================

    /**
     * Initialize Bootstrap tooltips for booked seats
     */
    function initializeTooltips() {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (el) {
            // Dispose old tooltip if exists to prevent duplicates
            var existingTooltip = bootstrap.Tooltip.getInstance(el);
            if (existingTooltip) {
                existingTooltip.dispose();
            }
            return new bootstrap.Tooltip(el);
        });
    }

    /**
     * Re-initialize tooltips when overlay opens (for dynamically rendered buttons)
     */
    function attachTooltipReinitListener() {
        document.addEventListener('click', function(ev) {
            const target = ev.target;
            // When any overlay becomes visible or link clicked
            if (target.closest('.overlay') || target.closest('[href^="#"]')) {
                setTimeout(initializeTooltips, 50);
            }
        });
    }

    // ============================================================
    // RUN INITIALIZATION ON PAGE LOAD
    // ============================================================

    // Initialize on load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            init();
            attachTooltipReinitListener();
        });
    } else {
        init();
        attachTooltipReinitListener();
    }
})();
