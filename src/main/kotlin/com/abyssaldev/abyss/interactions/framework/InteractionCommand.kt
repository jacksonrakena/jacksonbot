package com.abyssaldev.abyss.interactions.framework

import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandOption
import net.dv8tion.jda.api.EmbedBuilder
import net.dv8tion.jda.api.MessageBuilder
import java.util.*

abstract class InteractionCommand: InteractionBase, InteractionExecutable {
    open val options: Array<InteractionCommandOption> = emptyArray()

    override fun invoke(call: InteractionRequest): MessageBuilder?
        = respond("There was an error processing your subcommand.")

    override fun createMap(): HashMap<String, Any> {
        val hashMapInit = hashMapOf<String, Any>(
            "name" to name,
            "description" to description
        )
        if (options.any()) {
            hashMapInit["options"] = options.map {
                it.createMap()
            }
        }
        return hashMapInit
    }

    fun MessageBuilder.content(content: String) = apply {
        setContent(content)
    }

    fun MessageBuilder.embed(builder: EmbedBuilder.() -> Unit) = apply {
        val embedBuilder = EmbedBuilder()
        builder(embedBuilder)
        if (embedBuilder.build().color == null && AppConfig.instance.appearance.defaultEmbedColorObject != null) {
            embedBuilder.setColor(AppConfig.instance.appearance.defaultEmbedColorObject)
        }
        setEmbed(embedBuilder.build())
    }

    fun EmbedBuilder.appendDescriptionLine(line: String) = apply {
        this.descriptionBuilder.appendLine(line)
    }
}