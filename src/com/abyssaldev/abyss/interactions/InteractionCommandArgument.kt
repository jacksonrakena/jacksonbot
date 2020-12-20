package com.abyssaldev.abyss.interactions

enum class InteractionCommandArgumentType(val raw: Int) {
    Subcommand(0),
    SubcommandGroup(1),
    String(3),
    Integer(4),
    Boolean(5),
    User(6),
    Channel(7),
    Role(8)
}

data class InteractionCommandArgumentChoice(val name: String, val value: String)

data class InteractionCommandArgument(val name: String, val description: String, val type: InteractionCommandArgumentType, val isRequired: Boolean, val choices: Array<InteractionCommandArgumentChoice>? = null) {
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (javaClass != other?.javaClass) return false

        other as InteractionCommandArgument

        if (name != other.name) return false
        if (description != other.description) return false
        if (type != other.type) return false
        if (isRequired != other.isRequired) return false
        if (choices != null) {
            if (other.choices == null) return false
            if (!choices.contentEquals(other.choices)) return false
        } else if (other.choices != null) return false

        return true
    }

    override fun hashCode(): Int {
        var result = name.hashCode()
        result = 31 * result + description.hashCode()
        result = 31 * result + type.hashCode()
        result = 31 * result + isRequired.hashCode()
        result = 31 * result + (choices?.contentHashCode() ?: 0)
        return result
    }
}
