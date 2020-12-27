package com.abyssaldev.abyss.gateway.commands

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.util.write
import com.jagrosh.jdautilities.command.Command
import com.jagrosh.jdautilities.command.CommandEvent

class AdminCommandSet: Command() {
    init {
        name = "admin"
        hidden = true
        help = "The administrator command set."
        children = arrayOf(
            DebugPrintCommand(),
            GenerateInviteLinkCommand(),
            DumpInteractionJsonCommand(),
            DumpInteractionInfoCommand(),
            ExecuteDatabaseCommand()
        )
        ownerCommand = true
    }

    override fun execute(event: CommandEvent?) {
        event?.replySuccess("Available: `${children.map { it.name }.joinToString(", ")}`")
    }

    class ExecuteDatabaseCommand : Command() {
        init {
            name = "exec-db"
            help = "Executes a database command."
            ownerCommand = true
        }

        override fun execute(event: CommandEvent?) {
            event?.replyError("Database not connected.")
        }
    }

    class DumpInteractionInfoCommand: Command() {
        init {
            name = "debug-interactions"
            help = "Prints interactions debug information."
            ownerCommand = true
        }

        override fun execute(event: CommandEvent?) {
            event?.replySuccess("Successful (POST): ${AbyssEngine.instance.interactions.successfulReceivedInteractionRequests}, failed (POST): ${AbyssEngine.instance.interactions.failedReceivedInteractionRequests}")
        }
    }

    class DebugPrintCommand : Command() {
        init {
            name = "debug"
            help = "Prints debug information."
            ownerCommand = true
        }

        override fun execute(event: CommandEvent?) {
            event?.replySuccess("Debug")
        }
    }

    class GenerateInviteLinkCommand : Command() {
        init {
            name = "generate-invite"
            help = "Generates an invite link for this bot instance."
            ownerCommand = true
        }

        override fun execute(event: CommandEvent?) {
            event?.replySuccess("<https://discord.com/api/oauth2/authorize?client_id=${AbyssEngine.instance.discordEngine.selfUser.id}&permissions=0&scope=bot%20applications.commands>")
        }
    }

    class DumpInteractionJsonCommand : Command() {
        init {
            name = "dump-interaction-json"
            help = "Dumps the JSON for an interaction."
            ownerCommand = true
        }

        override fun execute(event: CommandEvent?) {
            val command = AbyssEngine.instance.interactions.getAllCommands().filter {
                it::class.simpleName == event?.args
            }[0]
            event?.replySuccess("`${command::class.simpleName}`: ```json\n" + AbyssEngine.jsonEngine.write(command.toJsonMap()) + "```")
        }
    }
}