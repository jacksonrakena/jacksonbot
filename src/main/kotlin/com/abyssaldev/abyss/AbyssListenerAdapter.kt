package com.abyssaldev.abyss

import com.abyssaldev.abyss.requests.AbyssCommandRequest
import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.rowi.core.results.CommandExceptionResult
import com.abyssaldev.rowi.core.results.CommandNotFoundResult
import com.abyssaldev.rowi.core.results.NotEnoughParametersResult
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.events.ReadyEvent
import net.dv8tion.jda.api.events.message.MessageReceivedEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter

class AbyssListenerAdapter(val engine: AbyssEngine): ListenerAdapter(), Loggable {
    override fun onReady(event: ReadyEvent) {
        logger.info("Received Discord READY signal. Connected as ${event.jda.selfUser}")
        GlobalScope.launch {
            AbyssEngine.instance.interactions.registerAllInteractions()
            AppConfig.instance.writeToDisk()
        }
    }

    override fun onMessageReceived(event: MessageReceivedEvent) {
        GlobalScope.launch {
            handleMessageReceived(event)
        }
    }

    private suspend fun handleMessageReceived(event: MessageReceivedEvent) {
        val prefix = AppConfig.instance.discord.gatewayPrefix
        if (!event.message.contentRaw.startsWith(prefix, ignoreCase = true)) return
        val result = engine.commandEngine.executeSuspending(
            event.message.contentRaw.substring(prefix.length),
            AbyssCommandRequest(event.message)
        )

        when (result) {
            is NotEnoughParametersResult -> {
                val appendix = if (result.suppliedParameterCount == 0) {
                    "but I didn't get any."
                } else {
                    "but I only got ${result.suppliedParameterCount}."
                }
                event.channel.sendMessage(StringBuilder()
                    .appendLine(":x: I needed ${result.expectedParameterCount} parameters after `${result.command.name}`, $appendix")
                    .appendLine()
                    .appendLine("For reference, here's how to use `${result.command.name}`:")
                    .appendLine("`" + prefix + result.command.name + " " + result.command.parameters.joinToString(
                        " "
                    ) { c ->
                        "[" + c.name + "]"
                    } + "`")
                    .toString()
                ).queue()
            }
            is CommandExceptionResult -> {
                logger.error("Exception during ${result.command.name}!", result.throwable)
                event.channel.sendMessage(":x: ${result.reason}").queue()
            }
            is CommandNotFoundResult -> {
                // Ignored result
            }
            else -> {
                when {
                    !result.isSuccess -> {
                        event.channel.sendMessage(":x: ${result.reason}").queue()
                    }
                }
            }
        }
    }
}