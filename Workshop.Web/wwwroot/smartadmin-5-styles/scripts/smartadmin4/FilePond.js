FilePond.registerPlugin(
    FilePondPluginImagePreview,
    FilePondPluginFileValidateSize,
    FilePondPluginFileValidateType,
    FilePondPluginFileEncode
);

FilePond.create(document.querySelector('#upload-basic'));
FilePond.create(document.querySelector('#upload-multiple'), {
    allowMultiple: true,
    maxFiles: 5,
    maxFileSize: '5MB',
    acceptedFileTypes: ['image/*', 'application/pdf']
});
FilePond.create(document.querySelector('#upload-preview'), {
    allowMultiple: true,
    allowImagePreview: true
});
FilePond.create(document.querySelector('#upload-encode'), {
    allowEncode: true
});