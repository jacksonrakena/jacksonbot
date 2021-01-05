package com.abyssaldev.abyss.commands.gateway

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.util.respondSuccess
import com.abyssaldev.abyss.util.write
import com.abyssaldev.rowi.core.contracts.ArgumentContract
import com.abyssaldev.rowi.core.reflect.Command
import com.abyssaldev.rowi.core.reflect.Name
import com.abyssaldev.rowi.jda.JdaCommandRequest
import com.abyssaldev.rowi.jda.JdaCommandResponse
import com.abyssaldev.rowi.jda.JdaModule
import com.abyssaldev.rowi.jda.impl.NotCallerContract
import io.ktor.client.request.*
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.EmbedBuilder
import net.dv8tion.jda.api.entities.Member
import java.time.Instant
import kotlin.reflect.jvm.jvmName

@Name("Admin")
class AdminModule: JdaModule() {
    private val actions = mapOf<String, JdaCommandRequest.(List<String>) -> String>(
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
        },
        "throwex" to {
            throw Error("Yeet.")
        }
    )

    @Command(
        name = "test",
        description = "Test."
    )
    fun invoke(call: JdaCommandRequest, @ArgumentContract(NotCallerContract.id) hello: Member, world: Int) = respond {
        setContent("hello=${hello} world=${world}")
    }

    @Command(
        name = "admin",
        description = "Provides administrative functions."
    )
    fun invoke(call: JdaCommandRequest, actionName: String): JdaCommandResponse = respond {
        if (actions.containsKey(actionName)) {
            val action = actions[actionName]!!
            val startTime = System.currentTimeMillis()
            val response = action.invoke(call, call.rawArgs.drop(1))
            val time = System.currentTimeMillis() - startTime

            respondSuccess("Executed action `${actionName}` in `${time}`ms.\n${response}")
        } else {
            respondSuccess("Available: `${actions.keys.joinToString(", ")}`")
        }
    }

    @Command(
        name = "ping",
        description = "Pong."
    )
    fun pingCommand(call: JdaCommandRequest) {
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
                it.channel.sendMessage(EmbedBuilder().apply {
                    setTitle("Pong!")
                    setDescription(message.toString())
                    setTimestamp(Instant.now())
                }.build()).queue()
            }
        }
    }
}