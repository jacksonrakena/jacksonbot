package com.abyssaldev.abyss.commands.interactions

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.framework.interactions.InteractionCommand
import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import net.dv8tion.jda.api.MessageBuilder

class HelpCommand : InteractionCommand() {
    override val name = "help"
    override val description: String = "Shows a list of available commands."

    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): MessageBuilder {
        val commands = AbyssEngine.instance.interactions.getAllCommands()

        return respondEmbed {
            setTitle("Commands")
            appendDescriptionLine("Remember, you can type `/` and commands will show up above your chat bar.")
            addField("Slash commands", commands.joinToString(", ") {
                "`/" + it.name + "`"
            }, true)
        }

    }
}