# RELEASE NOTES 1.0.0.10

Release Date: 2017-11-10

## Features

1. Add (manual) fraud check support by implementing callbacks
2. Support FROZEN payment status, stored on payment object in Epi
 After creating an order, call IResursBankCallbackClient.ProcessFrozenPayments(order) to validate any frozen payments and set the correct order/payment status.
2. Store ResursBankPaymentId on payment for reference
3. Implement fraud callbacks: UNFREEZE and ANNULMENT
  Requires public endpoint, creating an (api)controller is necessary
  Add two AppSettings, the endpoint url and a [salt](https://test.resurs.com/docs/display/ecom/Callbacks#Callbacks-Digestandchecksumdigest):
```XML
  <appSettings>
    <add key="ResursBank:CallbackUrl" value="https://[domain]/api/resurscallback?paymentId={paymentId}&amp;digest={digest}&amp;eventType={eventType}"/>
    <add key="ResursBank:CallbackDigestSalt" value="CreateYourOwnSalt"/>
  </appSettings>
```

<details>
  <summary>Example api controller</summary>

```csharp
 public class ResursCallbackController : ApiController
{
    private readonly ILogger _logger = LogManager.GetLogger(typeof(ResursCallbackController));
    private readonly IResursBankCallbackClient _resursBankCallbackClient;

    public ResursCallbackController(IResursBankCallbackClientFactory factory)
    {
        _resursBankCallbackClient = factory.Init(new ResursCredential(
            ConfigurationManager.AppSettings["ResursBank:UserName"],
            ConfigurationManager.AppSettings["ResursBank:Password"]
        ));
    }

    [Route("api/resurscallback")]
    [HttpGet]
    public IHttpActionResult Get(string eventType, string paymentId, string digest)
    {
        try
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            if (paymentId == null) throw new ArgumentNullException(nameof(paymentId));
            if (digest == null) throw new ArgumentNullException(nameof(digest));

            var type = (CallbackEventType)Enum.Parse(typeof(CallbackEventType), eventType);
            var data = new CallbackData
            {
                EventType = type,
                PaymentId = paymentId
            };

            _resursBankCallbackClient.ProcessCallback(data, digest);
            return StatusCode(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.Error($"ProcessCallback failed: {eventType}-{paymentId}-{digest}", ex);
        }
        
        return BadRequest("Could not process callback");
    }
}
</details>
  
## Shortcomings
Does not support other [callbacks](https://test.resurs.com/docs/display/ecom/Callbacks) yet.
