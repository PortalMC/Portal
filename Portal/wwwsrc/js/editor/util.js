export function checkExtension(input, extension) {
    if (!input.endsWith(`.${extension}`)) {
        return input + `.${extension}`;
    }
    return input;
}