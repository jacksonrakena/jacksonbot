package com.abyssaldev.abyss.framework.common

import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.Role
import net.dv8tion.jda.api.entities.TextChannel
import net.dv8tion.jda.api.entities.User

open class ArgumentSet(internal val argSet: List<String>, internal open val request: CommandRequest) {
    fun ordinal(position: Int): ArgumentValue? {
        val str = argSet.getOrNull(position)
        return if (str != null) {
            ArgumentValue(str, request)
        } else {
            null
        }
    }

    class Named(val argSetNamed: HashMap<String, String>, override val request: InteractionCommandRequest) : ArgumentSet(
        argSetNamed.values.toList(),
        request
    ) {
        fun named(name: String): ArgumentValue.Named {
            return ArgumentValue.Named(argSetNamed[name]!!, request)
        }
    }

    open class ArgumentValue(private val value: String, private val request: CommandRequest) {
        open val member: Member? by lazy {
            request.guild?.getMemberById(value)
        }

        open val user: User? by lazy {
            request.jda.getUserById(value)
        }

        open val channel: TextChannel? by lazy {
            request.jda.getTextChannelById(value)
        }

        open val integer: Int? by lazy {
            value.toIntOrNull()
        }

        open val long: Long? by lazy {
            value.toLongOrNull()
        }

        open val boolean: Boolean? by lazy {
            if (value != "true" && value != "false") {
                null
            } else {
                value.toBoolean()
            }
        }

        open val role: Role? by lazy {
            request.guild?.getRoleById(value)
        }

        val string: String = value

        class Named(private val value: String, private val request: InteractionCommandRequest) : ArgumentValue(value, request) {
            override val member: Member by lazy {
                request.guild.getMemberById(value)!!
            }

            override val user: User by lazy {
                request.jda.getUserById(value)!!
            }

            override val channel: TextChannel by lazy {
                request.jda.getTextChannelById(value)!!
            }

            override val integer: Int by lazy {
                value.toInt()
            }

            override val long: Long by lazy {
                value.toLong()
            }

            override val boolean: Boolean by lazy {
                value.toBoolean()
            }

            override val role: Role by lazy {
                request.guild.getRoleById(value)!!
            }
        }
    }
}