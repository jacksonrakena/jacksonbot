package com.abyssaldev.abyss

import com.abyssaldev.abyss.http.modules.mainModuleWeb
import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.abyss.util.Responder
import com.abyssaldev.abyss.util.time
import com.goterl.lazycode.lazysodium.LazySodiumJava
import com.goterl.lazycode.lazysodium.SodiumJava
import io.ktor.application.*
import io.ktor.server.engine.*
import io.ktor.server.netty.*
import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.JDABuilder
import net.dv8tion.jda.api.entities.Activity
import net.dv8tion.jda.api.events.ReadyEvent
import net.dv8tion.jda.api.events.message.MessageReceivedEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter
import net.dv8tion.jda.api.requests.RestAction

class AbyssApplication private constructor() : Loggable {
    companion object {
        // AbyssApplication is a singleton instance, lazy initialised
        val instance: AbyssApplication by lazy {
            AbyssApplication()
        }

        val lazySodiumInstance: LazySodiumJava by lazy {
            LazySodiumJava(SodiumJava())
        }
    }

    // HTTP server for Discord interactions and web API/control panel
    val httpServerEngine: NettyApplicationEngine

    // Discord engine
    lateinit var discordEngine: JDA
    private val discordEngineBuilder: JDABuilder

    init {
        httpServerEngine = embeddedServer(Netty, port = 1566, module = Application::mainModuleWeb)
        discordEngineBuilder = JDABuilder
            .createDefault(AppConfig.instance.discord.botToken)
            .setActivity(Activity.playing("a.help or /abysshelp"))
            .addEventListeners(AbyssDiscordListenerAdapter())

        Runtime.getRuntime().addShutdownHook(Thread {
            discordEngine.shutdownNow()
        })

        RestAction.setPassContext(true)
    }

    fun startAll() {
        val elapsed = time {
            httpServerEngine.start(wait = false)
            discordEngine = discordEngineBuilder.build()
        }

        logger.info("Started HTTP and Discord engines in ${elapsed}ms.")
        logger.info("Abyss initialisation complete.");
    }
}

class AbyssDiscordListenerAdapter: ListenerAdapter(), Loggable, Responder {
    override fun onReady(event: ReadyEvent) {
        logger.info("Received Discord READY signal.")
    }

    override fun onMessageReceived(event: MessageReceivedEvent) {
        if (event.message.contentRaw == "aj.test") {
            respond(
                event.textChannel.sendMessage("pog")
            )
        }
    }
}