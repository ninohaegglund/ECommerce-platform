namespace OrderService.Api.DTOs;

public class CreateAddressDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;

    public string StreetLine1 { get; set; } = string.Empty;
    public string StreetLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "SE";

    public string PhoneNumber { get; set; } = string.Empty;
}