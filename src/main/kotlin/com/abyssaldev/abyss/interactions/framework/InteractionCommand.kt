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

    override fun toJsonMap(): HashMap<String, Any> = hashMapOf(
        "name" to name,
        "description" to description,
        "options" to options.toJsonArray()
    )

    fun MessageBuilder.content(content: String) = apply {
        setContent(content)
    }

    fun MessageBuilder.embed(builder: EmbedBuilder.() -> Unit) = apply {
        setEmbed(EmbedBuilder().apply(builder).apply {
            if (this.build().color == null && AppConfig.instance.appearance.defaultEmbedColorObject != null) {
                setColor(AppConfig.instance.appearance.defaultEmbedColorObject)
            }
        }.build())
    }

    fun EmbedBuilder.appendDescriptionLine(line: String) = apply {
        this.descriptionBuilder.appendLine(line)
    }
}