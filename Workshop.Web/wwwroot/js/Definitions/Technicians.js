
//    $(document).ready(function () {
//        $("#TeamsForm").validate({
//            rules: {
//                TechnicianName: {
//                    required: true
//                },
//                TechnicianSkills: {
//                    required: true
//                },
//                TechnicianEfficiency: {
//                    required: true
//                },
//                TechnicianShift: {
//                    required: true
//                }
//            },
//            messages: {
//                Code: {
//                    required: resources.required_field
//            },
//                ShortName: {
//                    required: resources.required_field
//            },
//                Description: {
//                    required: resources.required_field
//            },
//                Color: {
//                    required: resources.required_field
//            }
//            },
//            errorClass: "text-danger",
//            errorElement: "span",
//            highlight: function (element) {
//                $(element).addClass("is-invalid");
//            },
//            unhighlight: function (element) {
//                $(element).removeClass("is-invalid");
//            },
//           errorPlacement: function (error, element) {
//               if (element.hasClass("select2-hidden-accessible")) error.insertAfter(element.next('.select2'));
//           }
//        });
//});