package com.abyssaldev.abyss

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.util.Loggable
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.events.ReadyEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter

class DiscordReadyListenerAdapter: ListenerAdapter(), Loggable {
    override fun onReady(event: ReadyEvent) {
        logger.info("Received Discord READY signal. Connected as ${event.jda.selfUser}")
        GlobalScope.launch {
            AbyssEngine.instance.interactions.registerAllInteractions()
            AppConfig.instance.writeToDisk()
        }
    }
}