package com.abyssaldev.abyss.http

import com.abyssaldev.abyss.AbyssEngine
import io.ktor.application.*
import io.ktor.response.*
import io.ktor.routing.*

class IndexRouting {
    companion object {
        fun Application.indexRouting() {
            routing {
                get("/invite") {
                    return@get call.respondRedirect(
                        "https://discord.com/api/oauth2/authorize?client_id=${AbyssEngine.instance.discordEngine.selfUser.id}&permissions=0&scope=bot%20applications.commands",
                        permanent = false
                    )
                }

                get("/discord") {
                    return@get call.respondRedirect(
                        "https://discord.gg/KACyc4XS7X",
                        permanent = false
                    )
                }

                get("/") {
                    return@get call.respondRedirect(
                        "https://github.com/abyssal/abyss",
                        permanent = false
                    )
                }
            }
        }
    }
}