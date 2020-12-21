package com.abyssaldev.abyss.http

import com.abyssaldev.abyss.AbyssEngine
import io.ktor.application.*
import io.ktor.response.*
import io.ktor.routing.*

class IndexRouting {
    companion object {
        private val pingAcknowledgeHashMap = hashMapOf("type" to 1)

        fun Application.indexRouting() {
            routing {
                get("/invite") {
                    if (AbyssEngine.instance.applicationInfo == null) {
                        return@get call.respondRedirect(
                            "https://github.com/abyssal/abyss",
                            permanent = false
                        )
                    }
                    return@get call.respondRedirect(
                        "https://discord.com/api/oauth2/authorize?client_id=${AbyssEngine.instance.applicationInfo!!.id}&permissions=0&scope=bot%20applications.commands",
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