package com.abyssaldev.abyss.commands.gateway

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.util.respondSuccess
import com.abyssaldev.abyss.util.write
import com.abyssaldev.abyssal_command_engine.framework.common.CommandModule
import com.abyssaldev.abyssal_command_engine.framework.common.reflect.Name
import com.abyssaldev.abyssal_command_engine.framework.gateway.GatewayCommandRequest
import com.abyssaldev.abyssal_command_engine.framework.gateway.command.GatewayCommand
import io.ktor.client.features.*
import io.ktor.client.request.*
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.MessageBuilder
import java.time.Instant
import kotlin.reflect.jvm.jvmName

@Name("Admin")
class AdminModule: CommandModule() {
    private val actions = mapOf<String, GatewayCommandRequest.(List<String>) -> String>(
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
            "`${command::class.jvmName}`: ```json\n" + AbyssEngine.jsonEngine.write(command.toJsonMap()) + "```"
        }
    )

    @GatewayCommand(
        name = "test",
        description = "Test."
    )
    fun invoke(call: GatewayCommandRequest, hello: String, world: Int) = respond {
        setContent("hello=${hello} world=${world}")
    }

    @GatewayCommand(
        name = "admin",
        description = "Provides administrative functions."
    )
    fun invoke(call: GatewayCommandRequest, actionName: String): MessageBuilder = respond {
        /*if (actionName.isNullOrEmpty()) {
            return@respond respondSuccess("Available: `${actions.keys.joinToString(", ")}`")
        }*/
        val action = actions[actionName]
            ?: return@respond respondSuccess("Available: `${actions.keys.joinToString(", ")}`")

        val startTime = System.currentTimeMillis()
        val response = action.invoke(call, call.args.asList().drop(1))
        val time = System.currentTimeMillis() - startTime

        val message = "Executed action `${actionName}` in `${time}`ms.\n${response}"
        return@respond respondSuccess(message)
    }

    @GatewayCommand(
        name = "ping",
        description = "Pong."
    )
    fun pingCommand(call: GatewayCommandRequest) {
        val message = StringBuilder().apply {
            appendLine(":handshake: **Gateway:** ${AbyssEngine.instance.discordEngine.gatewayPing}ms")
        }
        val initial = message.toString()

        val startTime = System.currentTimeMillis()
        call.channel.sendMessage("Pinging...").queue {
            val restSendTime = System.currentTimeMillis() - startTime
            message.appendLine(":mailbox_with_mail: **REST:** ${restSendTime}ms")
            GlobalScope.launch {
                val editStartTime = System.currentTimeMillis()
                AbyssEngine.instance.httpClientEngine.get<String>("http://google.com") {}
                val editEndTime = System.currentTimeMillis() - editStartTime
                message.appendLine(":globe_with_meridians: **Internet - Google:** ${editEndTime}ms")
                val acsStartTime = System.currentTimeMillis()
                AbyssEngine.instance.httpClientEngine.get<String>("https://live.abyssaldev.com/api/v1/hello") {}
                message.appendLine(":blue_heart: **Internet - Abyssal Services:** ${(System.currentTimeMillis() - acsStartTime)}ms")
                it.delete().queue()
                it.channel.sendMessage(respondEmbed {
                    setTitle("Pong!")
                    setDescription(message.toString())
                    setTimestamp(Instant.now())
                }.apply { setContent("") }.build()).queue()

            }
        }
    }
}