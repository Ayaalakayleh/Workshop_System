// ===== i18n & URLs from Razor =====
(function bootstrapI18n() {
    const root = document.getElementById("techDashboardRoot");
    const el = document.getElementById("i18n");

    window.i18n = {
        vehicleWorkshopSystem: el?.dataset.vehicleworkshopsystem || "Vehicle Workshop System",
        choosePinType: el?.dataset.choosepintype || "Choose PIN Type",
        pattern: el?.dataset.pattern || "Pattern",
        numericPin: el?.dataset.numericpin || "Numeric PIN",
        drawPatternToUnlock: el?.dataset.drawpatterntounlock || "Draw your pattern to unlock",
        tryAgain: el?.dataset.tryagain || "Try Again",
        enter6DigitPin: el?.dataset.enter6digitpin || "Enter 6-digit PIN",
        clear: el?.dataset.clear || "Clear",
        delete: el?.dataset.delete || "Delete",
        close: el?.dataset.close || "Close",
        numericKeyboard: el?.dataset.numerickeyboard || "Numeric Keyboard",
        pin: el?.dataset.pin || "PIN",
        nowor: el?.dataset.nowor || "NOWOR",
        timeUnknown: el?.dataset.timeunknown || "",

        // messages
        pinCorrect: el?.dataset.pincorrectredirecting || "PIN correct! Redirecting...",
        pinIncorrect: el?.dataset.incorrectpintryagain || "Incorrect PIN. Try again.",
        patternTooShort: el?.dataset.patterntooshortconnectatleast4 || "Pattern too short. Connect at least 4 dots.",
        patternCorrect: el?.dataset.patterncorrectredirecting || "Pattern correct! Redirecting...",
        patternIncorrect: el?.dataset.incorrectpatterntryagain || "Incorrect pattern. Try again."
    };
})();

// ===== Demo Technicians (replace with API data) =====
let technicians = [];

// ===== State =====
let selectedTechnician = null;
let patternDots = [];
let selectedDots = [];
let isDrawing = false;
let canvas, ctx;
let enteredPin = "";


function buildTechPhotoUrl(tech) {
    const base = window.RazorVars?.getImageUrl || '/Base/GetImage';
    const url = new URL(base, window.location.origin);

    const fileName = tech?.fileName || '';
    // IMPORTANT: remove leading "/" or "\" so server-side Path.Combine doesn't break
    const basePath = (tech?.filePath || '').replace(/^~?[\\/]+/, '');

    url.searchParams.set('Name', fileName);
    url.searchParams.set('D', '17');
    url.searchParams.set('BasePath', basePath);
    url.searchParams.set('IsExternal', 'false');

    return url.toString();
}

// ===== Utilities =====
function updateTime() {
    const now = new Date();
    const timeString = now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
    const el = document.getElementById('currentTime');
    if (el) el.textContent = timeString;
}

// Normalize technician object from server (support camelCase or PascalCase)
function normalizeTech(raw) {
    if (!raw) return null;
    return {
        id: raw.Id ?? raw.id ?? raw.ID ?? raw.EmployeeId ?? null,
        primaryName: raw.PrimaryName ?? raw.primaryName ?? raw.Name ?? raw.name ?? '',
        filePath: raw.FilePath ?? raw.filePath ?? raw.PhotoPath ?? raw.photoPath ?? '',
        fileName: raw.FileName ?? raw.fileName ?? '',
        hasPhoto: raw.HasPhoto ?? raw.hasPhoto ?? !!(raw.FilePath || raw.filePath),
        pattern: raw.Pattern ?? raw.pattern ?? null,
        numericPin: raw.PIN ?? raw.pin ?? raw.NumericPin ?? raw.numericPin ?? null,
        time: raw.Time ?? raw.time ?? null,
        status: raw.Status ?? raw.status ?? 'normal'
    };
}

function createTechnicianCard(tech, index) {
    const statusClass = tech.status === 'available' ? 'available' : tech.status === 'busy' ? 'busy' : 'normal';

    const statusText = tech.status === 'available' ? i18n.nowor : tech.status === 'busy' ? (tech.time || i18n.nowor) : i18n.nowor;

    const uploadsBasePath = buildTechPhotoUrl(tech);


    const photoContent = `<img src="${uploadsBasePath}" alt="${tech.id}" class="technician-photo">`;

    return `
    <div class="technician-card ${statusClass}" data-index="${index}">
      <div class="status-badge ${statusClass}"></div>
      <div class="technician-icon">${photoContent}</div>
      <div class="technician-info">
        <div class="technician-id">${tech.id}, ${escapeHtml(tech.primaryName)}</div>
        <div class="technician-status">${escapeHtml(statusText)}</div>
        ${tech.time ? `<div class="technician-time">${escapeHtml(tech.time)}</div>` : ''}
      </div>
    </div>`;
}

function escapeHtml(s) {
    if (!s) return '';
    return String(s).replace(/[&<>"']/g, function (c) { return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]; });
}

function renderTechnicians() {
    const grid = document.getElementById('techniciansGrid');
    if (!grid) return;
    const normalized = technicians.map(t => normalizeTech(t)).filter(Boolean);
    technicians = normalized; // replace with normalized list

    grid.innerHTML = technicians.map((tech, idx) => createTechnicianCard(tech, idx)).join('');
    document.querySelectorAll('.technician-card').forEach(card => {
        card.addEventListener('click', (e) => {
            const index = parseInt(e.currentTarget.getAttribute('data-index'));
            openPinTypeModal(technicians[index]);
        });
    });
}

// ===== Modals =====
function openPinTypeModal(technician) {
    console.log(technician);
    selectedTechnician = technician;
    const modal = document.getElementById('pinTypeModal');
    if (!modal) return;
    const nameEl = document.getElementById('pinTypeName');
    if (nameEl) nameEl.textContent = `${technician.id} - ${technician.primaryName}`;

    const hiddenInput = document.getElementById('technicianHidden');
    if (hiddenInput) {
        // Ensure the object sent to server includes a PIN property named 'PIN' and Id
        const techForServer = Object.assign({}, selectedTechnician, {
            PIN: selectedTechnician.numericPin != null ? Number(selectedTechnician.numericPin) : null,
            Id: selectedTechnician.id
        });
        hiddenInput.value = JSON.stringify(techForServer);
    }

    const modalPhoto = document.getElementById('pinTypePhoto');
    if (modalPhoto) {
        if (technician.hasPhoto) {
            modalPhoto.innerHTML = `<img src="${buildTechPhotoUrl(technician)}" alt="Technician">`;
            modalPhoto.classList.remove('no-photo');
        } else {
            modalPhoto.innerHTML = '🔧';
            modalPhoto.classList.add('no-photo');
        }
    }

    modal.classList.add('active');
    modal.setAttribute('aria-hidden', 'false');
}
function closePinTypeModal() {
    const modal = document.getElementById('pinTypeModal');
    if (!modal) return;
    modal.classList.remove('active');
    modal.setAttribute('aria-hidden', 'true');
}

function openPatternModal() {
    closePinTypeModal();
    const modal = document.getElementById('patternModal');
    if (!modal) return;
    const nameEl = document.getElementById('modalName');
    if (nameEl) nameEl.textContent = `${selectedTechnician.id} - ${selectedTechnician.primaryName}`;

    const modalPhoto = document.getElementById('modalPhoto');
    if (modalPhoto) {
        if (selectedTechnician.hasPhoto) {
            modalPhoto.innerHTML = '<img src="' + (selectedTechnician.filePath || '') + '" alt="Technician">';
            modalPhoto.classList.remove('no-photo');
        } else {
            modalPhoto.innerHTML = '🔧';
            modalPhoto.classList.add('no-photo');
        }
    }

    modal.classList.add('active');
    modal.setAttribute('aria-hidden', 'false');
    initializePattern();
}
function closePatternModal() {
    const modal = document.getElementById('patternModal');
    if (!modal) return;
    modal.classList.remove('active');
    modal.setAttribute('aria-hidden', 'true');
    selectedTechnician = null;
    selectedDots = [];
}

function openNumericModal() {
    closePinTypeModal();
    const modal = document.getElementById('numericModal');
    if (!modal) return;
    const nameEl = document.getElementById('numericName');
    if (nameEl) nameEl.textContent = `${selectedTechnician.id} - ${selectedTechnician.primaryName}`;

    const photo = document.getElementById('numericPhoto');
    if (photo) {
        if (selectedTechnician.hasPhoto) {
            photo.innerHTML = '<img src="' + (selectedTechnician.filePath || '') + '" alt="Technician">';
            photo.classList.remove('no-photo');
        } else {
            photo.innerHTML = '🔧';
            photo.classList.add('no-photo');
        }
    }

    enteredPin = '';
    updatePinDisplay();
    document.getElementById('numericError').textContent = '';
    modal.classList.add('active');
    modal.setAttribute('aria-hidden', 'false');
}
function closeNumericModal() {
    const modal = document.getElementById('numericModal');
    if (!modal) return;
    modal.classList.remove('active');
    modal.setAttribute('aria-hidden', 'true');
    selectedTechnician = null;
    enteredPin = '';
}

// ===== Numeric PIN =====
function updatePinDisplay() {
    const dots = document.querySelectorAll('.pin-dot');
    dots.forEach((dot, index) => dot.classList.toggle('filled', index < enteredPin.length));
}
function handleNumericInput(num) {
    if (enteredPin.length < 6) {
        enteredPin += num;
        updatePinDisplay();
        if (enteredPin.length === 6) checkNumericPin();
    }
}
function handleDeletePin() {
    if (enteredPin.length > 0) {
        enteredPin = enteredPin.slice(0, -1);
        updatePinDisplay();
    }
}
function handleClearPin() {
    enteredPin = '';
    updatePinDisplay();
    const el = document.getElementById('numericError'); if (el) el.textContent = '';
}

async function checkNumericPin() {
    const errorDiv = document.getElementById('numericError');
    if (!selectedTechnician) return;

    // Disable keypad while checking
    document.querySelectorAll('.num-btn[data-num]').forEach(b => b.setAttribute('disabled', 'disabled'));
    const ok = await checkPin(enteredPin);

    if (ok) {
        if (errorDiv) { errorDiv.style.color = '#10b981'; errorDiv.textContent = i18n.pinCorrect; }

        // POST technician to server to initialize session model, then redirect to Clocking GET
        try {
            console.log(getTechnician());
            const technicianObj = getTechnician();
            const resp = await fetch(window.RazorVars.clockingUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(technicianObj)
            });

            // After server processed, redirect to GET Clocking page which reads session
            window.location.href = window.RazorVars.clockingUrl;
        } catch (err) {
            console.error('Error sending technician to server:', err);
            if (errorDiv) { errorDiv.style.color = '#dc2626'; errorDiv.textContent = 'Server error'; }
        }
    } else {
        if (errorDiv) { errorDiv.style.color = '#dc2626'; errorDiv.textContent = i18n.pinIncorrect; }
        setTimeout(handleClearPin, 1500);
    }

    document.querySelectorAll('.num-btn[data-num]').forEach(b => b.removeAttribute('disabled'));
}

// Ensure this returns a JS object, not a JSON string
function getTechnician() {
    const technicianJson = document.getElementById('technicianHidden')?.value;
    let technician = technicianJson ? JSON.parse(technicianJson) : null;

    if (!gTechnician) return {};
    try { return gTechnician } catch (e) { return {}; }
}

// ===== Pattern Lock =====
function initializePattern() {
    canvas = document.getElementById('patternCanvas');
    if (!canvas) return;
    ctx = canvas.getContext('2d');

    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width * window.devicePixelRatio;
    canvas.height = rect.height * window.devicePixelRatio;
    ctx.scale(window.devicePixelRatio, window.devicePixelRatio);

    const gridSize = 3;
    const spacing = Math.min(rect.width, rect.height) / 4;
    const startX = (rect.width - spacing * 2) / 2;
    const startY = (rect.height - spacing * 2) / 2;

    patternDots = [];
    for (let row = 0; row < gridSize; row++) {
        for (let col = 0; col < gridSize; col++) {
            patternDots.push({ x: startX + col * spacing, y: startY + row * spacing, index: row * gridSize + col, selected: false });
        }
    }

    selectedDots = [];
    drawPattern();

    canvas.addEventListener('mousedown', handleStart);
    canvas.addEventListener('mousemove', handleMove);
    canvas.addEventListener('mouseup', handleEnd);
    canvas.addEventListener('touchstart', handleStart, { passive: false });
    canvas.addEventListener('touchmove', handleMove, { passive: false });
    canvas.addEventListener('touchend', handleEnd);
}
function drawPattern(currentX, currentY) {
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    ctx.clearRect(0, 0, rect.width, rect.height);

    ctx.strokeStyle = '#3b82f6';
    ctx.lineWidth = 3;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';

    if (selectedDots.length > 0) {
        ctx.beginPath();
        ctx.moveTo(patternDots[selectedDots[0]].x, patternDots[selectedDots[0]].y);
        for (let i = 1; i < selectedDots.length; i++) ctx.lineTo(patternDots[selectedDots[i]].x, patternDots[selectedDots[i]].y);
        if (currentX && currentY) ctx.lineTo(currentX, currentY);
        ctx.stroke();
    }

    patternDots.forEach((dot, index) => {
        const isSelected = selectedDots.includes(index);
        ctx.beginPath();
        ctx.arc(dot.x, dot.y, isSelected ? 18 : 14, 0, Math.PI * 2);
        ctx.fillStyle = isSelected ? '#3b82f6' : '#e5e7eb';
        ctx.fill();

        if (isSelected) {
            ctx.beginPath();
            ctx.arc(dot.x, dot.y, 8, 0, Math.PI * 2);
            ctx.fillStyle = '#ffffff';
            ctx.fill();
        }
    });
}
function getCanvasCoordinates(e) {
    const rect = canvas.getBoundingClientRect();
    const touch = e.touches ? e.touches[0] : e;
    return { x: touch.clientX - rect.left, y: touch.clientY - rect.top };
}
function getDotAtPosition(x, y) {
    const threshold = 30;
    return patternDots.findIndex(dot => {
        const dx = dot.x - x, dy = dot.y - y;
        return Math.sqrt(dx * dx + dy * dy) < threshold;
    });
}
function handleStart(e) {
    if (e.type === 'touchstart') e.preventDefault();
    isDrawing = true;
    const coords = getCanvasCoordinates(e);
    const idx = getDotAtPosition(coords.x, coords.y);
    if (idx !== -1 && !selectedDots.includes(idx)) { selectedDots.push(idx); drawPattern(); }
}
function handleMove(e) {
    if (!isDrawing) return; if (e.type === 'touchmove') e.preventDefault();
    const coords = getCanvasCoordinates(e);
    const idx = getDotAtPosition(coords.x, coords.y);
    if (idx !== -1 && !selectedDots.includes(idx)) selectedDots.push(idx);
    drawPattern(coords.x, coords.y);
}
function handleEnd() {
    if (!isDrawing) return; isDrawing = false; drawPattern(); checkPattern();
}
function checkPattern() {
    const errorDiv = document.getElementById('patternError');
    if (!selectedTechnician) return;
    if (selectedDots.length < 4) {
        if (errorDiv) { errorDiv.style.color = '#dc2626'; errorDiv.textContent = i18n.patternTooShort; }
        setTimeout(resetPattern, 1500); return;
    }

    const expected = selectedTechnician.pattern || null;
    if (!expected) {
        if (errorDiv) { errorDiv.style.color = '#dc2626'; errorDiv.textContent = 'Pattern not set for this user.'; }
        setTimeout(resetPattern, 1500); return;
    }

    const isCorrect = selectedDots.length === expected.length && selectedDots.every((d, i) => d === expected[i]);

    if (isCorrect) {
        if (errorDiv) { errorDiv.style.color = '#10b981'; errorDiv.textContent = i18n.patternCorrect; }
        // Initialize session on server then redirect like numeric PIN
        try {
            const technicianObj = selectedTechnician;
            fetch(window.RazorVars.clockingUrl, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(technicianObj) })
                .then(() => { window.location.href = window.RazorVars.clockingUrl; })
                .catch(err => console.error(err));
        } catch (err) { console.error(err); }
    } else {
        if (errorDiv) { errorDiv.style.color = '#dc2626'; errorDiv.textContent = i18n.patternIncorrect; }
        setTimeout(resetPattern, 1500);
    }
}
function resetPattern() { selectedDots = []; const err = document.getElementById('patternError'); if (err) err.textContent = ''; drawPattern(); }

// ===== Wire Up =====
document.getElementById('closePinType')?.addEventListener('click', closePinTypeModal);
document.getElementById('choosePattern')?.addEventListener('click', openPatternModal);
document.getElementById('chooseNumeric')?.addEventListener('click', openNumericModal);

document.getElementById('closePattern')?.addEventListener('click', closePatternModal);
document.getElementById('resetPattern')?.addEventListener('click', resetPattern);

document.getElementById('closeNumeric')?.addEventListener('click', closeNumericModal);
document.getElementById('clearPin')?.addEventListener('click', handleClearPin);
document.getElementById('deletePin')?.addEventListener('click', handleDeletePin);

document.querySelectorAll('.num-btn[data-num]').forEach(btn => { btn.addEventListener('click', () => handleNumericInput(btn.getAttribute('data-num'))); });

document.getElementById('pinTypeModal')?.addEventListener('click', (e) => { if (e.target.id === 'pinTypeModal') closePinTypeModal(); });
document.getElementById('patternModal')?.addEventListener('click', (e) => { if (e.target.id === 'patternModal') closePatternModal(); });
document.getElementById('numericModal')?.addEventListener('click', (e) => { if (e.target.id === 'numericModal') closeNumericModal(); });

updateTime();
setInterval(updateTime, 1000);
renderTechnicians();

// Fetch technicians from server
$(document).ready(function () {
    $.ajax({ type: 'GET', url: window.RazorVars.getTechniciansPINUrl, dataType: 'json' })
        .done(function (data) { technicians = data || []; renderTechnicians(); })
        .fail(function (err) { console.error('Failed to load technicians', err); });
});
var gTechnician; 
async function checkPin(PIN) {
    // Try to get technician from hidden input first, fall back to selectedTechnician (set when card clicked)
    const technicianJson = document.getElementById('technicianHidden')?.value;
    let technician = technicianJson ? JSON.parse(technicianJson) : null;

    if (!technician) {
        // use the in-memory selectedTechnician object if available
        technician = selectedTechnician || null;
    }

    if (!technician) {
        console.warn('checkPin: no technician available to verify');
        return false;
    }
    gTechnician = technician;
    try {
        // Build payload with correct property names expected by server
        const payload = {
            Technician: {
                Id: Number(technician?.Id ?? technician?.id ?? 0),
                PIN: Number(technician?.PIN ?? technician?.numericPin ?? 0)
            },
            PIN: Number(PIN)
        };

        const response = await $.ajax({
            type: 'POST',
            url: window.RazorVars.checkNumericPINUrl,
            dataType: 'json',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        });

        return response === true;
    } catch (error) {
        console.error('Error checking PIN:', error);
        return false;
    }
}
