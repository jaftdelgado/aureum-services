# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [proto/market.proto](#proto_market-proto)
    - [BuyAssetNotification](#market-BuyAssetNotification)
    - [BuyAssetRequest](#market-BuyAssetRequest)
    - [BuyAssetResponse](#market-BuyAssetResponse)
    - [MarketAsset](#market-MarketAsset)
    - [MarketRequest](#market-MarketRequest)
    - [MarketResponse](#market-MarketResponse)
    - [SellAssetRequest](#market-SellAssetRequest)
    - [SellAssetResponse](#market-SellAssetResponse)
  
    - [MarketService](#market-MarketService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="proto_market-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## proto/market.proto



<a name="market-BuyAssetNotification"></a>

### BuyAssetNotification



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_public_id | [string](#string) |  |  |
| message | [string](#string) |  |  |






<a name="market-BuyAssetRequest"></a>

### BuyAssetRequest



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| team_public_id | [string](#string) |  |  |
| asset_public_id | [string](#string) |  |  |
| user_public_id | [string](#string) |  |  |
| quantity | [double](#double) |  |  |
| price | [double](#double) |  |  |






<a name="market-BuyAssetResponse"></a>

### BuyAssetResponse



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| movement_public_id | [string](#string) |  |  |
| transaction_public_id | [string](#string) |  |  |
| transaction_price | [double](#double) |  |  |
| quantity | [double](#double) |  |  |
| notifications | [BuyAssetNotification](#market-BuyAssetNotification) | repeated |  |
| team_public_id | [string](#string) |  |  |






<a name="market-MarketAsset"></a>

### MarketAsset



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [string](#string) |  |  |
| symbol | [string](#string) |  |  |
| name | [string](#string) |  |  |
| price | [double](#double) |  |  |
| base_price | [double](#double) |  |  |
| volatility | [double](#double) |  |  |






<a name="market-MarketRequest"></a>

### MarketRequest



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| interval_seconds | [int32](#int32) |  |  |
| team_public_id | [string](#string) |  |  |






<a name="market-MarketResponse"></a>

### MarketResponse



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| timestamp_unix_millis | [int64](#int64) |  |  |
| assets | [MarketAsset](#market-MarketAsset) | repeated |  |






<a name="market-SellAssetRequest"></a>

### SellAssetRequest



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| team_public_id | [string](#string) |  |  |
| asset_public_id | [string](#string) |  |  |
| user_public_id | [string](#string) |  |  |
| quantity | [double](#double) |  |  |
| price | [double](#double) |  |  |






<a name="market-SellAssetResponse"></a>

### SellAssetResponse



| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| movement_public_id | [string](#string) |  |  |
| transaction_public_id | [string](#string) |  |  |
| transaction_price | [double](#double) |  |  |
| quantity | [double](#double) |  |  |
| notifications | [BuyAssetNotification](#market-BuyAssetNotification) | repeated |  |
| team_public_id | [string](#string) |  |  |





 

 

 


<a name="market-MarketService"></a>

### MarketService
====== servicio ======

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| CheckMarket | [MarketRequest](#market-MarketRequest) | [MarketResponse](#market-MarketResponse) stream |  |
| BuyAsset | [BuyAssetRequest](#market-BuyAssetRequest) | [BuyAssetResponse](#market-BuyAssetResponse) |  |
| SellAsset | [SellAssetRequest](#market-SellAssetRequest) | [SellAssetResponse](#market-SellAssetResponse) |  |

 



## Scalar Value Types

| .proto Type | Notes | C++ | Java | Python | Go | C# | PHP | Ruby |
| ----------- | ----- | --- | ---- | ------ | -- | -- | --- | ---- |
| <a name="double" /> double |  | double | double | float | float64 | double | float | Float |
| <a name="float" /> float |  | float | float | float | float32 | float | float | Float |
| <a name="int32" /> int32 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint32 instead. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="int64" /> int64 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint64 instead. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="uint32" /> uint32 | Uses variable-length encoding. | uint32 | int | int/long | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="uint64" /> uint64 | Uses variable-length encoding. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum or Fixnum (as required) |
| <a name="sint32" /> sint32 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int32s. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sint64" /> sint64 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int64s. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="fixed32" /> fixed32 | Always four bytes. More efficient than uint32 if values are often greater than 2^28. | uint32 | int | int | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="fixed64" /> fixed64 | Always eight bytes. More efficient than uint64 if values are often greater than 2^56. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum |
| <a name="sfixed32" /> sfixed32 | Always four bytes. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sfixed64" /> sfixed64 | Always eight bytes. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="bool" /> bool |  | bool | boolean | boolean | bool | bool | boolean | TrueClass/FalseClass |
| <a name="string" /> string | A string must always contain UTF-8 encoded or 7-bit ASCII text. | string | String | str/unicode | string | string | string | String (UTF-8) |
| <a name="bytes" /> bytes | May contain any arbitrary sequence of bytes. | string | ByteString | str | []byte | ByteString | string | String (ASCII-8BIT) |

