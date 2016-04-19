# Spryng .NET API Library

This library helps with developing against the Spryng SMS Api.


## Installation & Initialization

You can use NuGet to easily install the library:

```
Install-Package SpryngApiHttpDotNet
```

To use the Spryng HTTP Api you must first create a new instance of the `SpryngHttpClient`:

```C#
SpryngHttpClient client = new SpryngHttpClient(username, password);
```


## Sending a SMS

To send a SMS, you must create a `SmsRequest` object. You can create one like this:

```C#
SmsRequest request = new SmsRequest()
{
    Destinations = new string[] { "31612345678", "31698765421" },
    Sender = "Spryng",
    Body = "This is a Test SMS."
};

```

You can now send the SMS using the client:

```C#
try
{
    client.ExecuteSmsRequest(request);
    Console.WriteLine("SMS has been send!");
}
catch (SpryngHttpClientException ex)
{
    Console.WriteLine("An Exception occured!\n{0}", ex.Message);
}
```

The API also provides an Async implementation which can be used in the same fashion as the synchronous api:

```C#
await client.ExecuteSmsRequestAsync(request);
```

#### SmsRequest Options

There are multiple properties available in the `SmsRequest` that can be changed.

* `Destinations` A string array of phone numbers you're sending the sms to.
* `Body` Body of the sms.
* `Sender` Originator address, like your company name.
* `Route` Whether to use Spryng Business, Spryng Economy or a custom route. Defaults to `business`.
* `Reference` An optional reference for delivery reports.
* `AllowLong` Whether you want to allow long SMS or not. Defaults to `False`. 

## Request Credit Balance

It's also possible to request the accounts credit balance using the client. This method is available sycnhronously and asynchronously.

```C#
// Synchronous
double remainingCredits = client.GetCreditAmount();

// Asynchronous
double remainingCredits = await client.GetCreditAmountAsync();
```
