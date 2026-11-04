# API Contract (v0.1, verified from my controller source)

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
| POST | add-category | No (gap) |
| PUT | update-category | No (gap) |
| DELETE | delete-category/{id} | No (gap) |

## Products (/api/products)
| Method | Route | Auth |
|---|---|---|
| GET | get-all | No |
| GET | get-by-id/{id} | No |
| POST | Add-Product | No (gap) |
| PUT | Update-Product | No (gap) |
| DELETE | Delete-Product/{Id} | No (gap) |

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