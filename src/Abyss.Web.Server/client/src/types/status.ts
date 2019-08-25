export interface BotStatus {
    addonsLoaded: number;
    avatarUrl: string;
    channels: number;
    commandFailures: number;
    commandSuccesses: number;
    commands: number;
    contentRootPath: string;
    culture: string;
    currentThreadId: number;
    environment: string;
    guilds: number;
    machineName: string;
    modules: number;
    operatingSystem: string;
    processName: string;
    processorCount: number;
    runtimeVersion: string;
    serviceName: string;
    startTime: string;
    usernameDiscriminator: string;
    users: number;
}