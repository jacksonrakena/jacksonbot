package com.abyssaldev.abyss.gateway

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.util.Loggable
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.events.ReadyEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter

class AbyssDiscordListenerAdapter: ListenerAdapter(), Loggable {
    override fun onReady(event: ReadyEvent) {
        logger.info("Received Discord READY signal. Connected as ${event.jda.selfUser.toString()}")
        logger.info("Starting interaction controller...")
        AbyssApplication.instance.discordEngine.retrieveApplicationInfo().queue {
            if (it == null) {
                logger.error("Failed to retrieve application information. Cannot register interactions.")
                return@queue
            }
            AbyssApplication.instance.applicationInfo = it
            logger.info("Retrieved application info. Registering interactions with application ID ${it.id}")
            GlobalScope.launch {
                AbyssApplication.instance.interactions.registerAllInteractions(it)
            }
        }
    }
}