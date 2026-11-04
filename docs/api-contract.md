# API Contract (v0.2, verified from my controller source)

Catalogue reads are anonymous. Catalogue writes require a bearer token
with the `admin` realm role from Keycloak. Requests without a token
receive 401, and authenticated requests without the role receive 403.

## Account (/api/account)
| Method | Route | Auth |
|---|---|---|
| GET | get-address-for-user | Not confirmed yet |
| GET | Logout | No |
| GET | get-user-name | Yes |
| PUT | update-address | Yes |
| POST | Register | No |
| POST | Login | No |
| POST | active-account | No |
| GET | send-email-forget-password | No |
| POST | reset-password | No |

## Categories (/api/categories)
| Method | Route | Auth |
|---|---|---|
| GET | get-all | No |
| GET | get-by-id/{id} | No |
| POST | add-category | Yes, Admin |
| PUT | update-category | Yes, Admin |
| DELETE | delete-category/{id} | Yes, Admin |

## Products (/api/products)
| Method | Route | Auth |
|---|---|---|
| GET | get-all | No |
| GET | get-by-id/{id} | No |
| POST | Add-Product | Yes, Admin |
| PUT | Update-Product | Yes, Admin |
| DELETE | Delete-Product/{Id} | Yes, Admin |

### Product list query

`GET /api/products` accepts `sort` (`priceAsc` or `priceDesc`; any other value
falls back to id order), `categoryId`, `search`, `pageIndex` and `pageSize`.
Every word in `search` must appear in the product name or description, ignoring
case. Pages are 1-based, `pageSize` is capped at 6, and values below 1 become 1.

The response is a page instead of a bare array:

```json
{
  "pageIndex": 1,
  "pageSize": 6,
  "totalCount": 10,
  "data": []
}
```

### Catalogue write responses

`POST /api/categories` and `POST /api/products` return `201 Created` with a
`Location` header pointing at the new resource and the created resource as its
response DTO. `PUT /api/categories/{id}` and `PUT /api/products/{id}` return
`200 OK` with the updated DTO. Both response DTOs include the resource id, so a
client does not need a second request to learn it.

## Baskets (/api/baskets)
| Method | Route | Auth |
|---|---|---|
| GET | get-basket-item/{id} | No (gap) |
| POST | update-basket | No (gap) |
| DELETE | delete-basket-item/{id} | No (gap) |

## Orders (/api/orders), entire controller requires auth
| Method | Route | Auth |
|---|---|---|
| POST | create-order | Yes |
| GET | get-orders-for-user | Yes |
| GET | get-order-by-id/{id} | Yes |
| GET | get-delivery | Yes |

## Payments (/api/payments)
| Method | Route | Auth |
|---|---|---|
| POST | Create | Yes |
| POST | webhook | No, correct since this is an external Stripe callback |

## Ratings (/api/ratings)
| Method | Route | Auth |
|---|---|---|
| GET | get-rating/{productId} | No |
| POST | add-rating | No (gap) |