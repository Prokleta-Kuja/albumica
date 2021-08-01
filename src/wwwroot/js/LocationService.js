let dotNet, result;

export function initialize(dotNetObj) {
    dotNet = dotNetObj;
}

export async function reverseGeoCode(uri) {
    result = {};

    await fetch(uri)
        .then(response => response.json())
        .catch(er => console.error(er))
        .then(data => {
            result.IsSuccess = true;
            result.CountryCode = data.address.country_code.trim();
            result.Country = data.address.country.trim();
            result.City = data.address.city.trim();
            result.Suburb = data.address.suburb.trim();
        });

    return result;
}