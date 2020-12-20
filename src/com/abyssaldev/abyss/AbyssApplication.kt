package com.abyssaldev.abyss

import com.abyssaldev.abyss.gateway.AbyssDiscordListenerAdapter
import com.abyssaldev.abyss.http.modules.DiscordInteractionRouting.Companion.discordInteractionRouting
import com.abyssaldev.abyss.interactions.InteractionController
import com.abyssaldev.abyss.interactions.commands.CatPictureCommand
import com.abyssaldev.abyss.interactions.commands.HelpCommand
import com.abyssaldev.abyss.interactions.commands.TextCommand
import com.abyssaldev.abyss.interactions.commands.UserInfoCommand
import com.abyssaldev.abyss.interactions.http.IndexRouting.Companion.indexRouting
import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.abyss.util.time
import com.fasterxml.jackson.databind.DeserializationFeature
import com.fasterxml.jackson.databind.ObjectMapper
import com.fasterxml.jackson.databind.SerializationFeature
import io.ktor.application.*
import io.ktor.client.*
import io.ktor.client.features.json.*
import io.ktor.features.*
import io.ktor.jackson.*
import io.ktor.request.*
import io.ktor.server.engine.*
import io.ktor.server.netty.*
import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.JDABuilder
import net.dv8tion.jda.api.entities.Activity
import net.dv8tion.jda.api.entities.ApplicationInfo
import net.dv8tion.jda.api.requests.RestAction
import net.dv8tion.jda.api.utils.MemberCachePolicy
import org.slf4j.event.Level

class AbyssApplication private constructor() : Loggable {
    companion object {
        // AbyssApplication is a singleton instance, lazy initialised
        val instance: AbyssApplication by lazy {
            AbyssApplication()
        }

        // JSON (de)serialization
        val objectMapper = ObjectMapper().apply {
            // Ensure that JSON deserialiation doesn't fail if we haven't mapped all the properties
            configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
        }
    }

    // Handles incoming interactions and registering slash commands over REST with Discord
    val interactions: InteractionController = InteractionController()

    // Cache application info after READY
    var applicationInfo: ApplicationInfo? = null

    // HTTP server for Discord interactions and web API/control panel
    val httpServerEngine: NettyApplicationEngine

    // HTTP client for Discord interactions and some commands
    val httpClientEngine: HttpClient

    // Discord engine for gateway presence
    lateinit var discordEngine: JDA
    private val discordEngineBuilder: JDABuilder

    init {
        httpServerEngine = embeddedServer(Netty, port = AppConfig.instance.web.port) {
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
            discordInteractionRouting()

            indexRouting()
        }

        httpClientEngine = HttpClient {
            install(JsonFeature) {
                serializer = JacksonSerializer(objectMapper)
            }
        }

        discordEngineBuilder = JDABuilder
            .createDefault(AppConfig.instance.discord.botToken)
            .setActivity(Activity.playing("/help"))
            .setMemberCachePolicy(MemberCachePolicy.ALL)
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
                TextCommand("ping", "Checks to see if I'm online.", "Pong!"),
                HelpCommand(),
                UserInfoCommand()
            )
        }

        logger.info("Added all commands to registration queue.")
        logger.info("Started HTTP and Discord engines in ${elapsed}ms.")
        logger.info("Abyss initialisation complete.")
    }
}