
// IntlTelInput instances
let instances = {};

// Example 1: Basic Phone Input
instances.basic = window.intlTelInput(document.querySelector("#basic-phone"), {
    initialCountry: "auto",
    geoIpLookup: function (success) {
        fetch('https://ipapi.co/json/')
            .then(response => response.json())
            .then(data => success(data.country_code))
            .catch(() => success('sa'));
    }
});

// Example 2: Auto Country Detection
instances.auto = window.intlTelInput(document.querySelector("#auto-country"), {
    initialCountry: "auto",
    geoIpLookup: function (success) {
        fetch('https://ipapi.co/json/')
            .then(response => response.json())
            .then(data => success(data.country_code))
            .catch(() => success('sa'));
    }
});

// Example 3: Preferred Countries
instances.preferred = window.intlTelInput(document.querySelector("#preferred-countries"), {
    preferredCountries: ['sa', 'jo', 'eg'],
    initialCountry: "sa"
});

// Example 4: Phone Validation
instances.validation = window.intlTelInput(document.querySelector("#validation-phone"), {
    initialCountry: "us"
});

const validationInput = document.querySelector("#validation-phone");
const validationResult = document.querySelector("#validation-result");

validationInput.addEventListener('input', function () {
    const isValid = instances.validation.isValidNumber();
    validationResult.style.display = 'block';

    if (this.value.trim() === '') {
        validationResult.style.display = 'none';
        return;
    }

    if (isValid) {
        validationResult.className = 'validation-result valid';
        validationResult.textContent = '✓ Valid phone number';
    } else {
        validationResult.className = 'validation-result invalid';
        const errorCode = instances.validation.getValidationError();
        let errorText = 'Invalid phone number';

        switch (errorCode) {
            case 1: errorText = 'Invalid country code'; break;
            case 2: errorText = 'Too short'; break;
            case 3: errorText = 'Too long'; break;
            case 4: errorText = 'Invalid format'; break;
        }

        validationResult.textContent = '✗ ' + errorText;
    }
});

// Example 5: Only Countries
instances.only = window.intlTelInput(document.querySelector("#only-countries"), {
    onlyCountries: ['sa', 'jo', 'eg'],
    initialCountry: "sa"
});

// Example 6: Phone Info Display
instances.info = window.intlTelInput(document.querySelector("#info-phone"), {
    initialCountry: "sa"
});

const infoInput = document.querySelector("#info-phone");
const infoDisplay = document.querySelector("#phone-info");

infoInput.addEventListener('input', function () {
    if (this.value.trim() === '') {
        infoDisplay.innerHTML = `
                    <strong>Phone Information:</strong><br>
                    International: -<br>
                    National: -<br>
                    Country: -<br>
                    Valid: -<br>
                    Type: -
                `;
        return;
    }

    const selectedCountryData = instances.info.getSelectedCountryData();
    const international = instances.info.getNumber();
    const national = instances.info.getNumber(window.intlTelInputUtils.numberType.NATIONAL);
    const isValid = instances.info.isValidNumber();
    const numberType = isValid ? instances.info.getNumberType() : 'Unknown';

    let typeString = 'Unknown';
    if (numberType !== 'Unknown') {
        const types = ['Fixed line', 'Mobile', 'Fixed line or mobile', 'Toll free', 'Premium rate', 'Shared cost', 'VOIP', 'Personal number', 'Pager', 'UAN', 'Voicemail'];
        typeString = types[numberType] || 'Unknown';
    }

    infoDisplay.innerHTML = `
                <strong>Phone Information:</strong><br>
                International: ${international}<br>
                National: ${national}<br>
                Country: ${selectedCountryData.name} (${selectedCountryData.iso2.toUpperCase()})<br>
                Valid: ${isValid ? 'Yes' : 'No'}<br>
                Type: ${typeString}
            `;
});

// Example 7: National Mode
instances.national = window.intlTelInput(document.querySelector("#national-mode"), {
    nationalMode: true,
    initialCountry: "sa"
});

// Example 8: Custom Placeholder
instances.placeholder = window.intlTelInput(document.querySelector("#placeholder-phone"), {
    autoPlaceholder: "aggressive",
    initialCountry: "sa"
});

// Example 9: Format Options
instances.format = window.intlTelInput(document.querySelector("#format-phone"), {
    initialCountry: "sa"
});

function getNumber(format) {
    const formatResult = document.querySelector("#format-result");
    const input = document.querySelector("#format-phone");

    if (!input.value.trim()) {
        formatResult.style.display = 'block';
        formatResult.textContent = 'Please enter a phone number first.';
        return;
    }

    let number;
    switch (format) {
        case 'E164':
            number = instances.format.getNumber();
            break;
        case 'NATIONAL':
            number = instances.format.getNumber(window.intlTelInputUtils.numberFormat.NATIONAL);
            break;
        case 'INTERNATIONAL':
            number = instances.format.getNumber(window.intlTelInputUtils.numberFormat.INTERNATIONAL);
            break;
    }

    formatResult.style.display = 'block';
    formatResult.innerHTML = `<strong>${format} Format:</strong><br>${number || 'Invalid number'}`;
}

// Example 10: Exclude Countries
instances.exclude = window.intlTelInput(document.querySelector("#exclude-countries"), {
    excludeCountries: ['sa', 'jo', 'eg'],
    initialCountry: "sa"
});
