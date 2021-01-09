package com.abyssaldev.abyss.commands.gateway

import com.abyssaldev.rowi.core.CommandResponse
import com.abyssaldev.rowi.core.reflect.Command
import com.abyssaldev.rowi.jda.JdaCommandRequest
import com.abyssaldev.rowi.jda.JdaModule

class UtilModule : JdaModule() {
    @Command(name = "spotify", description = "Shows your playing track.")
    fun spotifyCommand(call: JdaCommandRequest): CommandResponse = respond {
        val spotify = call.member?.activities?.first { it.isRich }?.asRichPresence()
        if (spotify == null) {
            append(":x: You're not listening to anything.")
            return@respond
        }
        append(spotify.toString())
    }
}