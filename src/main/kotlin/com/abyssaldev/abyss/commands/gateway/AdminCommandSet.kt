package com.abyssaldev.abyss.commands.gateway

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.util.write
import com.jagrosh.jdautilities.command.Command
import com.jagrosh.jdautilities.command.CommandEvent

class AdminCommandSet: Command() {
    val actions = mapOf<String, CommandEvent.(List<String>) -> String>(
        "exec-db" to {
            "Database not connected."
        },
        "debug-interactions" to {
            "Successful (POST): ${AbyssEngine.instance.interactions.successfulReceivedInteractionRequests}, failed (POST): ${AbyssEngine.instance.interactions.failedReceivedInteractionRequests}"
        },
        "generate-invite" to {
            "<https://discord.com/api/oauth2/authorize?client_id=${AbyssEngine.instance.discordEngine.selfUser.id}&permissions=0&scope=bot%20applications.commands>"
        },
        "dump-interaction-json" to {
            val command = AbyssEngine.instance.interactions.getAllCommands().filter { c ->
                c::class.simpleName == it[0]
            }[0]
            "`${command::class.simpleName}`: ```json\n" + AbyssEngine.jsonEngine.write(command.toJsonMap()) + "```"
        }
    )

    init {
        name = "admin"
        hidden = true
        help = "The administrator command set."
        ownerCommand = true
    }

    override fun execute(event: CommandEvent?) {
        if (event == null) return
        if (event.args.isNullOrEmpty()) {
            return event.replySuccess("Available: `${actions.keys.joinToString(", ")}`")
        }
        val args = event.args.split(" ")
        if (!actions.containsKey(args[0])) {
            return event.replySuccess("Available: `${actions.keys.joinToString(", ")}`")
        }
        val action = actions[args[0]]
            ?: return event.replySuccess("Available: `${actions.keys.joinToString(", ")}`")

        val startTime = System.currentTimeMillis()
        val response = action.invoke(event, args.drop(1))
        val time = System.currentTimeMillis() - startTime

        val message = "Executed action `${args[0]}` in `${time}`ms.\n${response}"
        event.replySuccess(message)
    }
}