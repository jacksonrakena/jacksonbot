package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import com.abyssaldev.abyss.interactions.framework.InteractionRequest
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandOption
import com.fasterxml.jackson.annotation.JsonProperty
import io.ktor.client.request.*
import net.dv8tion.jda.api.MessageBuilder
import java.time.Instant

class ExchangeCommand : InteractionCommand() {
    override val name = "exchange"
    override val description = "Converts one currency to another, using live exchange rates."

    override val options: Array<InteractionCommandOption> = arrayOf(
        InteractionCommandArgument(
            name = "amount",
            description = "The amount of currency to convert.",
            type = InteractionCommandArgumentType.Integer
        ),
        InteractionCommandArgument(
            name = "from",
            description = "The currency to convert from. For example, USD.",
            type = InteractionCommandArgumentType.String
        ),
        InteractionCommandArgument(
            name = "to",
            description = "The currency to convert to. For example, USD.",
            type = InteractionCommandArgumentType.String
        )
    )

    override suspend fun invoke(call: InteractionRequest): MessageBuilder {
        val amount = call.arguments[0].value.toInt()
        val from = call.arguments[1].value.toUpperCase()
        val to = call.arguments[2].value.toUpperCase()

        val url = "https://v6.exchangerate-api.com/v6/${AppConfig.instance.keys.exchangeRateApiKey}/latest/${from}"
        val response = AbyssEngine.instance.httpClientEngine.get<ExchangeRateApiResponse>(url)
        if (response.result != "success") return respond {
            content("Unknown currency.")
        }
        if (!response.conversionRates.containsKey(to)) return respond {
            content("Unknown currency.")
        }

        return respond {
            embed {
                setTitle(":currency_exchange: Currency exchange")
                appendDescriptionLine("${amount} **${response.originCurrency}** is ${response.conversionRates[to]!!*amount} **${to}**")
                setFooter("Exchange rate: 1 ${response.originCurrency} = ${response.conversionRates[to]} ${to}")
                setTimestamp(Instant.now())
            }
        }
    }

    data class ExchangeRateApiResponse(
        @JsonProperty("result") var result: String,
        @JsonProperty("base_code") var originCurrency: String,
        @JsonProperty("conversion_rates") var conversionRates: HashMap<String, Double>
    )
}