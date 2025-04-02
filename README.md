### Тестовое задание в HyperQuant

https://docs.google.com/document/d/1EC8PDKLaVA8T9RJdVP-i0FIQWbGkA17Ux-EBGFdgTDM/edit?tab=t.0

## Структура проекта

В `TestHQ` содержится контроллер `BitfinexController` для тестирования REST API методов клиента `BitfinexRestClient`.
В `TestHQ.Core` содержится реализация коннектора к бирже Bitfinex `BitfinexConnector`. В качестве его компонентов были созданы два клиента: `BitfinexRestClient` для получения трейдов, свечей и тикеров и `BitfinexWebSocketClient` для подписки и отписки на трейды и свечи.
В `TestHQ.Tests` содержатся интеграционные тесты, которые проверяют работоспособность клиентов Bitfinex.
В `TestHQ.Wpf` содержится Desktop приложение, которое отображает состояние портфолио (1 BTC, 15000 XRP, 50 XMR и 30 DASH).