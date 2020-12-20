package com.abyssaldev.abyss

import com.abyssaldev.abyss.http.modules.discordInteractionModule
import com.abyssaldev.abyss.interactions.InteractionController
import com.abyssaldev.abyss.interactions.commands.CatPictureCommand
import com.abyssaldev.abyss.interactions.commands.TextCommand
import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.abyss.util.Responder
import com.abyssaldev.abyss.util.time
import com.fasterxml.jackson.databind.SerializationFeature
import io.ktor.application.*
import io.ktor.client.*
import io.ktor.client.features.json.*
import io.ktor.features.*
import io.ktor.jackson.*
import io.ktor.request.*
import io.ktor.server.engine.*
import io.ktor.server.netty.*
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.JDABuilder
import net.dv8tion.jda.api.entities.Activity
import net.dv8tion.jda.api.events.ReadyEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter
import net.dv8tion.jda.api.requests.RestAction
import org.slf4j.event.Level

class AbyssApplication private constructor() : Loggable {
    companion object {
        // AbyssApplication is a singleton instance, lazy initialised
        val instance: AbyssApplication by lazy {
            AbyssApplication()
        }
    }

    val interactions: InteractionController = InteractionController()

    // HTTP server for Discord interactions and web API/control panel
    val httpServerEngine: NettyApplicationEngine

    // HTTP client for Discord interactions and some commands
    val httpClientEngine: HttpClient

    // Discord engine
    lateinit var discordEngine: JDA
    private val discordEngineBuilder: JDABuilder

    init {
        httpServerEngine = embeddedServer(Netty, port = 1566) {
            install(ContentNegotiation) {
                jackson {
                    enable(SerializationFeature.INDENT_OUTPUT)
                }
            }

            install(CallLogging) {
                level = Level.INFO
                filter { call -> call.request.path().startsWith("/") }
            }

            // ROUTING MODULES
            discordInteractionModule()
        }

        httpClientEngine = HttpClient {
            install(JsonFeature) {
                serializer = JacksonSerializer()
            }
        }

        discordEngineBuilder = JDABuilder
            .createDefault(AppConfig.instance.discord.botToken)
            .setActivity(Activity.playing("/abysshelp"))
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

            interactions.addCommands(
                CatPictureCommand(),
                TextCommand("ping", "Checks to see if I'm online.", "Pong!")
            )
        }

        logger.info("Added all commands to registration queue.")
        logger.info("Started HTTP and Discord engines in ${elapsed}ms.")
        logger.info("Abyss initialisation complete.")
    }
}

class AbyssDiscordListenerAdapter: ListenerAdapter(), Loggable, Responder {
    override fun onReady(event: ReadyEvent) {
        logger.info("Received Discord READY signal.")
        logger.info("Starting interaction controller...")
        AbyssApplication.instance.discordEngine.retrieveApplicationInfo().queue {
            if (it == null) {
                logger.error("Failed to retrieve application information. Cannot register interactions.")
                return@queue
            }
            GlobalScope.launch {
                AbyssApplication.instance.interactions.registerAllCommandsInGuild(it, "385902350432206849")
            }
        }
    }
}