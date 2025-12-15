function EditTechnicians(ID) {
    window.location = RazorVars.editTechnicianUrl + '?Id=' + ID;
}

//----------------------------------------------------------------------------------------------------------------------------------------------//
$(document).ready(function () {
    // var maxBirthDate = new Date();
    // maxBirthDate.setFullYear(maxBirthDate.getFullYear() - 19);
    getData(1);

    // $("#BirthDate").flatpickr({
    //     dateFormat: "Y-m-d",
    //     maxDate: maxBirthDate,
    //     yearRange: [1950, new Date().getFullYear() - 19],
    //     defaultDate: null,
    //     allowInput: true,
    //     locale: { firstDayOfWeek: 1 }
    // });

    // var phoneInput = document.querySelector("#Phone");
    // var iti = window.intlTelInput(phoneInput, {
    //     hiddenInput: "full_number",
    //     preferredCountries: ['sa', 'jo', 'eg'],
    //     autoPlaceholder: 'off',
    //     separateDialCode: true,
    // });

    // ---------- FilePond ----------
    FilePond.registerPlugin(
        FilePondPluginImagePreview,
        FilePondPluginFileValidateType,
        FilePondPluginFileValidateSize,
        FilePondPluginImageExifOrientation,
        FilePondPluginImageResize,
        FilePondPluginImageTransform
    );
    const {
        imgUrl,
        label_UploadTechnicianPhoto
    } = window.RazorVars || {};

    //console.log("🔍 RazorVars.imgUrl:", imgUrl);
    //console.log("🪶 Upload Label:", label_UploadTechnicianPhoto);

    const pond = FilePond.create(document.querySelector('.TechnicianPhoto'), {
        labelIdle: label_UploadTechnicianPhoto,
        acceptedFileTypes: ['image/*'],
        maxFileSize: '5MB',
        imagePreviewHeight: 120,
        imageCropAspectRatio: '1:1',
        imageResizeTargetWidth: 150,
        imageResizeTargetHeight: 150,
        stylePanelLayout: 'circle',
        styleLoadIndicatorPosition: 'center bottom',
        styleProgressIndicatorPosition: 'right bottom',
        styleButtonRemoveItemPosition: 'left bottom',
        styleButtonProcessItemPosition: 'right bottom',

        files: imgUrl
            ? [{ source: imgUrl, options: { type: 'local' } }]
            : [],

        server: {
            load: (uniqueFileId, load) => {
                console.log("FilePond loading image from:", uniqueFileId);
                fetch(uniqueFileId)
                    .then(r => r.blob())
                    .then(blob => {
                        console.log("Image blob loaded:", blob);
                        load(blob);
                    })
                    .catch(err => console.error("Error loading image:", err));
            }
        }
    });



    // $.validator.addMethod("minAge", function (value, element, params) {
    //     if (!value) return false;
    //     var birthDate = new Date(value);
    //     var today = new Date();
    //     var age = today.getFullYear() - birthDate.getFullYear();
    //     var monthDiff = today.getMonth() - birthDate.getMonth();
    //     if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
    //         age--;
    //     }
    //     return age >= params;
    // }, RazorVars.val_BirthDateMinAge);

    // $.validator.addMethod("phoneValidation", function (value, element) {
    //     if (value.trim() === "") return false;
    //     return iti.isValidNumber();
    // }, RazorVars.val_PhoneRequired);

    // $.validator.addMethod("emailValidation", function (value, element) {
    //     return this.optional(element) || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
    // }, RazorVars.val_EmailRequired);

    // $.validator.addMethod("usernameValidation", function (value, element) {
    //     return this.optional(element) || /^[a-zA-Z0-9_]{3,20}$/.test(value);
    // }, RazorVars.val_UserNameRequired);

    $("#workshopform").validate({
        rules: {
            PrimaryName: { required: true },
            // SecondaryName: { required: true, minlength: 2, maxlength: 50 },
            // UserName: { required: true, usernameValidation: true },
            Email: { required: false },
            // Phone: { required: true, phoneValidation: true },
            // PrimaryAddress: { required: false },
            // SecondaryAddress: { required: false },
            // BirthDate: { required: true, date: true, minAge: 18 },
            PIN: { required: true, minlength: 6, maxlength: 6 },
            FK_SkillId: { required: true, minlength: 1 }
        },
        messages: {
            PrimaryName: { required: resources.required_field },
            // SecondaryName: {
            //     required: resources.required_field,
            //     minlength: RazorVars.val_SecondaryNameMin,
            //     maxlength: RazorVars.val_SecondaryNameMax
            // },
            // UserName: { required: resources.required_field },
            // Phone: { required: resources.required_field, phoneValidation: RazorVars.val_InvalidPhoneNumber },
            // BirthDate: {
            //     required: resources.required_field,
            //     date: RazorVars.val_BirthDateInvalid,
            //     minAge: RazorVars.val_BirthDateMinAge
            // },
            FK_SkillId: { required: resources.required_field }
        },
        errorElement: "span",
        errorClass: "text-danger", // use "invalid-feedback" if you're using Bootstrap feedback styling
        highlight: function (element) {
            $(element)
                .addClass("is-invalid")
                .removeClass("is-valid")
                .closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element)
                .removeClass("is-invalid")
                .closest('.form-group').removeClass('has-error');
        },
        // ✅ Important: actually insert the error node somewhere visible
        errorPlacement: function (error, element) {
            // Select2 (hidden input + visible .select2)
            if (element.hasClass("select2-hidden-accessible")) {
                const $s2 = element.next(".select2");
                if ($s2.length) {
                    $s2.next(".text-danger").remove(); // avoid duplicates
                    error.insertAfter($s2);
                } else {
                    error.insertAfter(element);
                }
                return;
            }

            // intl-tel-input wrapper (if enabled)
            if (element.attr("id") === "Phone" && element.closest(".iti").length) {
                error.insertAfter(element.closest(".iti"));
                return;
            }

            // FilePond (original input hidden; show after its root)
            if (element.hasClass("TechnicianPhoto")) {
                const pondRoot = element.closest(".filepond--root");
                if (pondRoot.length) {
                    pondRoot.next(".text-danger").remove();
                    error.insertAfter(pondRoot);
                } else {
                    error.insertAfter(element);
                }
                return;
            }

            // Default
            error.insertAfter(element);
        },
        submitHandler: function (form) {
            $("#btnCreate")
                .prop('disabled', true)
                .html('<i class="fa fa-spinner fa-spin"></i>&nbsp;' + RazorVars.btnSaving);

            var formData = new FormData(form);

            // attach photo from FilePond if present
            const pondFiles = pond.getFiles();
            if (pondFiles.length > 0) {
                formData.append('TechnicianPhoto', pondFiles[0].file);
            }

            // if you re-enable intl-tel-input, you can set the full number:
            // var fullPhoneNumber = iti.getNumber();
            // formData.set('Phone', fullPhoneNumber);

            $.ajax({
                url: $(form).attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function () {
                    $("#btnCreate")
                        .prop('disabled', false)
                        .html('<i class="fa fa-save"></i>&nbsp;' + RazorVars.btnSave);

                    Swal.fire({
                        icon: 'success',
                        title: resources.success_msg,
                        confirmButtonText: resources.ok,
                        confirmButtonColor: 'var(--primary-600)',
                        timer: 3000,
                        timerProgressBar: true
                    }).then(() => {
                        window.location.href = document.referrer;
                    });
                },
                error: function () {
                    $("#btnCreate")
                        .prop('disabled', false)
                        .html('<i class="fa fa-save"></i>&nbsp;' + RazorVars.btnSave);

                    Swal.fire({
                        icon: 'error',
                        title: RazorVars.ErrorHappend,
                        confirmButtonText: RazorVars.btnTryAgain,
                        confirmButtonColor: '#dc3545'
                    });
                }
            });

            return false;
        }
    });

    // live clean-up of invalid state + remove message next to proper element/container
    $("input, select, textarea").on('input change', function () {
        const $el = $(this);
        if ($el.hasClass('is-invalid')) {
            $el.removeClass('is-invalid').closest('.form-group').removeClass('has-error');

            // If Select2, the visible container is next('.select2')
            const $s2 = $el.next('.select2');
            if ($s2.length) {
                $s2.next('.text-danger').remove();
            } else {
                // Also handle FilePond wrapper cleanup
                const $pondRoot = $el.closest('.filepond--root');
                if ($pondRoot.length) {
                    $pondRoot.next('.text-danger').remove();
                } else {
                    $el.next('.text-danger').remove();
                }
            }
        }
    });

    // misc UI niceties
    $('[data-toggle="tooltip"]').tooltip();
    $('select[data-ajax="true"]').each(function () {
        var $select = $(this);
        if ($select.find('option').length <= 1) {
            $select.append('<option disabled>' + RazorVars.loadingText + '...</option>');
        }
    });
    $('textarea').each(function () {
        this.style.height = 'auto';
        this.style.height = this.scrollHeight + 'px';
    });

    function generateRandomPIN() {
        let pin = Math.floor(100000 + Math.random() * 900000);
        return pin;
    }
    var tempID = $("#Id").val();
    if (!(tempID > 0))
        $('#PIN').val(generateRandomPIN());

    // Optional: regenerate PIN on focus or button click
    // $('#PIN').on('focus', function () {
    //     $(this).val(generateRandomPIN());
    // });
});

// ---------- List load + pager ----------
function getData(page) {
    page = page || 1;
    var FilterModel = {
        'UserName': $('#_UserName').val(),
        'Name': $('#_Name').val(),
        'Email': $('#_Email').val(),
        'PageNumber': page
    };
    $.ajax({
        type: 'GET',
        url: RazorVars.indexTechnicianUrl,
        dataType: 'html',
        data: FilterModel
    }).done(function (result) {
        $("#contentList").html(result);
        if ($("#total_pages").val() != undefined) {
            $("#contentPager").attr("hidden", false);
            $('#contentPager').pagination('destroy');
            $('#contentPager').pagination({
                items: $("#total_pages").val(),
                itemOnPage: 25,
                currentPage: page,
                prevText: '&laquo;',
                nextText: '&raquo;',
                onPageClick: function (page) { getData(page); }
            });
            $('#contentPager ul').addClass('pagination');
            $('#contentPager li').addClass('page-item');
            $('#contentPager a, #contentPager span').addClass('page-link');
        } else {
            $("#contentPager").attr("hidden", true);
        }
    });
}
$(document).on('click', '.toggle-pin', function () {
    var $btn = $(this);
    var $input = $('#PIN');

    var isShown = $btn.attr('aria-pressed') === 'true';
    $btn.attr('aria-pressed', !isShown)
        .attr('aria-label', isShown ? 'Show PIN' : 'Hide PIN')
        .attr('title', isShown ? 'Show PIN' : 'Hide PIN');

    $input.attr('type', isShown ? 'password' : 'text');
    var el = $input[0];
    if (el) {
        var len = el.value.length;
        el.focus();
        try { el.setSelectionRange(len, len); } catch (e) { }
    }
});

$("#btnSearch").click(function () { getData(); });

$("#IsResigned").on("change", function () {
    if ($(this).is(":checked")) {
        $("#ResignedDate").prop("disabled", false);
    } else {
        $("#ResignedDate").prop("disabled", true).val("");
    }
});
