// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function createFormData(object: {[key: string]: any}, form?: FormData, namespace?: string): FormData {
    const formData = form ?? new FormData();
    for (const property in object) {
        if (!object.hasOwnProperty(property) || object[property] == null) {
            continue;
        }
        const formKey = namespace ? `${namespace}[${property}]` : property;
        if (object[property] instanceof Date) {
            formData.append(formKey, object[property].toISOString());
        } else if (typeof object[property] === 'object' && !(object[property] instanceof File)) {
            createFormData(object[property], formData, formKey);
        } else {
            formData.append(formKey, object[property]);
        }
    }
    return formData;
}
