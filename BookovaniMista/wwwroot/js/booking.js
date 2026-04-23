// -*- coding: utf-8 -*-
/**
 * Booking JavaScript Module
 * Handles seat booking functionality with confirmation dialog
 */

(function () {
    let currentButton = null;
    const dialog = document.getElementById('confirm-dialog');
    const messageEl = document.getElementById('confirm-message');
    const yesBtn = document.getElementById('confirm-yes');
    const cancelBtn = document.getElementById('confirm-cancel');
    const dateInput = document.getElementById('booking-date');

    // Validate required elements
    if (!dialog || !messageEl) {
        console.error('❌ Confirm dialog elements not found.');
        return;
    }

    /**
     * Handle seat button clicks - show confirmation dialog
     */
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
        // ✅ HTML generates data-sekce-db and data-section-title attributes
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

    /**
     * Handle cancel button
     */
    if (cancelBtn) {
        cancelBtn.addEventListener('click', function () {
            dialog.setAttribute('aria-hidden', 'true');
            dialog.style.display = 'none';
            currentButton = null;
        });
    }

    /**
     * Handle booking confirmation (YES button)
     */
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
            // ✅ HTML generates data-sekce-db attribute (not data-section-id)
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
            const headers = { 'Content-Type': 'application/json' };
            console.log('📡 Sending booking request...');
            fetch('/Akcni/Rezervovat', {
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

    /**
     * Close dialog on ESC key
     */
    document.addEventListener('keydown', function (ev) {
        if (ev.key === 'Escape' && dialog.getAttribute('aria-hidden') === 'false') {
            dialog.setAttribute('aria-hidden', 'true');
            dialog.style.display = 'none';
            currentButton = null;
        }
    });

    /**
     * Refresh page when date changes
     */
    if (dateInput) {
        dateInput.addEventListener('change', function () {
            const selectedDate = this.value;
            if (selectedDate) {
                window.location.href = '?bookingDate=' + encodeURIComponent(selectedDate);
            }
        });
    }
})();
