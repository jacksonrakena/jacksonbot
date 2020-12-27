package com.abyssaldev.abyss.commands.interactions

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.framework.interactions.InteractionCommand
import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption
import com.abyssaldev.abyss.util.TimedCacheable
import com.abyssaldev.abyss.util.round
import com.fasterxml.jackson.annotation.JsonProperty
import io.ktor.client.request.*
import net.dv8tion.jda.api.MessageBuilder
import java.time.Duration
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

    val rateMap: HashMap<String, TimedCacheable.Suspended<ExchangeRateApiResponse>> = hashMapOf()

    override suspend fun invoke(call: InteractionCommandRequest): MessageBuilder {
        val amount = call.args.named("amount").integer
        val from = call.args.named("from").string.toUpperCase()
        val to = call.args.named("to").string.toUpperCase()

        val ratesCachable = rateMap.getOrPut(from) {
            TimedCacheable.Suspended(fetch = {
                return@Suspended AbyssEngine.instance.httpClientEngine.get<ExchangeRateApiResponse>("https://v6.exchangerate-api.com/v6/${AppConfig.instance.keys.exchangeRateApiKey}/latest/${from}")
            }, Duration.ofHours(24).toMillis())
        }

        val cachableRates = ratesCachable.get()

        return respond {
            if ((cachableRates.result != "success") or !cachableRates.conversionRates.containsKey(to)) {
                content("Unknown currency.")
            } else {
                embed {
                    setTitle(":currency_exchange: Currency exchange")
                    appendDescriptionLine("${amount} **${cachableRates.originCurrency}** is ${(cachableRates.conversionRates[to]!!*amount) round 2} **${to}**")
                    setFooter("Exchange rate: 1 ${cachableRates.originCurrency} = ${cachableRates.conversionRates[to]} ${to}")
                    setTimestamp(Instant.now())
                }
            }
        }
    }

    data class ExchangeRateApiResponse(
        @JsonProperty("result") var result: String,
        @JsonProperty("base_code") var originCurrency: String,
        @JsonProperty("conversion_rates") var conversionRates: HashMap<String, Double>
    )
}