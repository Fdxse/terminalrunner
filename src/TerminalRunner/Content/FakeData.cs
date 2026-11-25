namespace TerminalRunner.Content;

/// <summary>
/// Word lists and generators for realistic looking Linux output.
/// Everything here is made up, but uses real looking names, paths and formats.
/// </summary>
internal static class FakeData
{
    public static readonly string[] UserNames =
    [
        "dev", "ops", "build", "deploy", "admin", "sysop", "jenkins", "runner",
    ];

    public static readonly string[] HostNames =
    [
        "prod-api-03", "build-node-12", "k8s-worker-07", "db-replica-02", "edge-gw-01",
        "ci-runner-04", "staging-web-05", "mx-relay-02", "ml-train-gpu-01", "nas-backup-01",
    ];

    public static readonly string[] AptPackages =
    [
        "libssl3t64", "libcurl4t64", "nginx", "nginx-common", "postgresql-16", "postgresql-client-16",
        "redis-server", "redis-tools", "libjemalloc2", "liblzf1", "python3-pip", "python3-venv",
        "python3-setuptools", "python3-wheel", "build-essential", "gcc-13", "g++-13", "make",
        "libc6-dev", "linux-libc-dev", "libgcc-13-dev", "cmake", "cmake-data", "ninja-build",
        "libarchive13t64", "librhash0", "libjsoncpp25", "libuv1t64", "git", "git-man", "liberror-perl",
        "htop", "libnl-3-200", "libnl-genl-3-200", "jq", "libjq1", "libonig5", "curl", "wget",
        "ca-certificates", "openssl", "docker-ce", "docker-ce-cli", "containerd.io",
        "docker-buildx-plugin", "docker-compose-plugin", "libltdl7", "pigz", "slirp4netns",
        "fail2ban", "python3-pyinotify", "whois", "unattended-upgrades", "rsync", "zstd",
        "libzstd-dev", "libprotobuf32t64", "protobuf-compiler", "libgrpc29t64", "mariadb-server",
        "mariadb-client", "galera-4", "libdbi-perl", "socat", "libdbd-mysql-perl", "tmux",
        "libevent-core-2.1-7t64", "libutempter0", "net-tools", "dnsutils", "bind9-libs",
    ];

    public static readonly string[] SystemdServices =
    [
        "Journal Service", "Network Time Synchronization", "Network Name Resolution",
        "D-Bus System Message Bus", "OpenBSD Secure Shell server", "Regular background program processing daemon",
        "Login Service", "Network Configuration", "containerd container runtime", "Docker Application Container Engine",
        "nginx - high performance web server", "PostgreSQL Cluster 16-main", "Advanced key-value store",
        "Dispatcher daemon for systemd-networkd", "Snap Daemon", "Permit User Sessions",
        "Unattended Upgrades Shutdown", "Authorization Manager", "Load Kernel Modules",
        "Create Static Device Nodes in /dev", "Rule-based Manager for Device Events and Files",
        "Flush Journal to Persistent Storage", "Apply Kernel Variables", "Remount Root and Kernel File Systems",
        "Set the console keyboard layout", "Create Volatile Files and Directories", "Record System Boot/Shutdown in UTMP",
        "Daily apt download activities", "Daily rotation of log files", "Discard unused blocks once a week",
        "Message of the Day", "Prometheus exporter for machine metrics", "MariaDB 11.4 database server",
        "Fail2Ban Service", "Getty on tty1", "Serial Getty on ttyS0", "irqbalance daemon",
        "LSB: automatic crash report generation", "Thermal Daemon Service", "Pollinate to seed the pseudo random number generator",
    ];

    public static readonly string[] SystemdTargets =
    [
        "Local File Systems", "Network", "Network is Online", "Basic System", "Sockets", "Timers",
        "Path Units", "Swaps", "Slice Units", "System Time Set", "Login Prompts", "Multi-User System",
        "Graphical Interface", "Host and Network Name Lookups", "Preparation for Network",
    ];

    public static readonly string[] KernelSourceDirectories =
    [
        "kernel/sched", "kernel/time", "kernel/irq", "kernel/bpf", "mm", "fs/ext4", "fs/btrfs", "fs/xfs",
        "fs/nfs", "net/ipv4", "net/ipv6", "net/core", "net/netfilter", "drivers/net/ethernet/intel/e1000e",
        "drivers/net/ethernet/mellanox/mlx5/core", "drivers/gpu/drm/amd/amdgpu", "drivers/gpu/drm/i915",
        "drivers/nvme/host", "drivers/usb/core", "drivers/usb/host", "drivers/pci", "drivers/acpi",
        "drivers/scsi", "drivers/md", "drivers/block", "arch/x86/kernel", "arch/x86/mm", "crypto", "security/selinux",
        "sound/pci/hda", "lib", "block", "ipc", "init",
    ];

    public static readonly string[] KernelSourceFiles =
    [
        "core", "main", "fair", "rt", "deadline", "inode", "super", "namei", "file", "dir", "extents",
        "balloc", "ialloc", "journal", "tcp_input", "tcp_output", "route", "af_inet", "skbuff", "dev",
        "netlink", "page_alloc", "slub", "vmscan", "memory", "mmap", "swapfile", "huge_memory", "pci",
        "probe", "irq", "msi", "dma", "ioctl", "sysfs", "debugfs", "trace", "hrtimer", "clocksource",
        "workqueue", "signal", "fork", "exit", "cpu", "smp", "power", "pm", "ethtool", "param", "verifier",
    ];

    public static readonly string[] RustCrates =
    [
        "proc-macro2", "unicode-ident", "quote", "syn", "serde", "serde_derive", "serde_json", "libc",
        "cfg-if", "autocfg", "memchr", "itoa", "ryu", "once_cell", "bytes", "pin-project-lite",
        "futures-core", "futures-util", "futures-channel", "tokio", "tokio-macros", "mio", "socket2",
        "log", "tracing", "tracing-core", "tracing-subscriber", "regex", "regex-syntax", "aho-corasick",
        "hashbrown", "indexmap", "smallvec", "parking_lot", "parking_lot_core", "lock_api", "scopeguard",
        "rand", "rand_core", "rand_chacha", "getrandom", "http", "http-body", "httparse", "hyper",
        "hyper-util", "tower", "tower-service", "axum", "axum-core", "matchit", "percent-encoding",
        "url", "idna", "chrono", "time", "uuid", "anyhow", "thiserror", "thiserror-impl", "clap",
        "clap_builder", "clap_derive", "clap_lex", "anstream", "strsim", "base64", "sha2", "digest",
        "ring", "rustls", "rustls-pki-types", "webpki-roots", "sqlx-core", "sqlx-macros", "sqlx-postgres",
    ];

    public static readonly string[] ProjectNames =
    [
        "telemetry-gateway", "order-service", "ledger-core", "edge-proxy", "billing-worker",
        "inventory-api", "auth-broker", "metrics-collector", "event-router", "search-indexer",
    ];

    public static readonly string[] GitHosts =
    [
        "github.com", "gitlab.com", "git.internal.lan",
    ];

    public static readonly string[] GitOrganizations =
    [
        "platform", "infra", "core-team", "data-eng", "sre", "backend",
    ];

    public static readonly string[] WebPaths =
    [
        "/", "/api/v1/health", "/api/v1/orders", "/api/v1/orders/{id}", "/api/v2/users/{id}",
        "/api/v2/session", "/static/js/app.{hash}.js", "/static/css/main.{hash}.css", "/metrics",
        "/favicon.ico", "/login", "/logout", "/api/v1/search?q=report", "/api/v1/inventory/{id}/stock",
        "/robots.txt", "/.well-known/acme-challenge/{hash}", "/api/v1/events", "/graphql",
    ];

    public static readonly string[] HttpMethods =
    [
        "GET", "GET", "GET", "GET", "POST", "POST", "PUT", "DELETE", "PATCH", "HEAD",
    ];

    public static readonly string[] UserAgents =
    [
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_6) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.1 Safari/605.1.15",
        "Mozilla/5.0 (X11; Linux x86_64; rv:132.0) Gecko/20100101 Firefox/132.0",
        "curl/8.5.0",
        "kube-probe/1.31",
        "Prometheus/2.54.1",
        "python-requests/2.32.3",
        "Go-http-client/1.1",
    ];

    public static readonly string[] PythonTestModules =
    [
        "tests/unit/test_models.py", "tests/unit/test_serializers.py", "tests/unit/test_utils.py",
        "tests/unit/test_pricing.py", "tests/unit/test_validation.py", "tests/integration/test_api.py",
        "tests/integration/test_database.py", "tests/integration/test_cache.py", "tests/integration/test_queue.py",
        "tests/e2e/test_checkout.py", "tests/e2e/test_auth_flow.py", "tests/unit/test_scheduler.py",
    ];

    public static readonly string[] DockerBaseImages =
    [
        "ubuntu:24.04", "debian:bookworm-slim", "python:3.12-slim", "node:22-alpine", "golang:1.23",
        "mcr.microsoft.com/dotnet/sdk:10.0", "rust:1.83-slim", "alpine:3.20",
    ];

    public static readonly string[] PingTargets =
    [
        "8.8.8.8", "1.1.1.1", "gateway.internal.lan", "db-replica-02", "registry.internal.lan", "9.9.9.9",
    ];

    public static readonly string[] KernelModules =
    [
        "nvme", "e1000e", "i915", "amdgpu", "snd_hda_intel", "iwlwifi", "btusb", "xhci_pci", "ahci",
        "kvm_intel", "kvm", "intel_rapl_msr", "overlay", "br_netfilter", "nf_conntrack", "ip_tables",
        "x_tables", "dm_crypt", "raid1", "md_mod", "zram", "efi_pstore", "crc32_pclmul", "aesni_intel",
    ];

    public static readonly string[] CpuFeatures =
    [
        "x87 floating point registers", "SSE registers", "AVX registers", "MPX bounds registers",
        "MPX CSR", "AVX-512 opmask", "AVX-512 Hi256", "AVX-512 ZMM_Hi256", "Protection Keys User registers",
    ];

    /// <summary>
    /// Produces one realistic kernel log (dmesg) message, without the timestamp.
    /// </summary>
    public static string KernelMessage(Random random)
    {
        // Each case is a different kind of message seen in a real boot log.
        return random.Next(22) switch
        {
            0 => $"pci 0000:{random.Hex(2)}:{random.Next(0, 32):x2}.{random.Next(0, 8)}: [{random.Hex(4)}:{random.Hex(4)}] type 00 class 0x{random.Hex(6)} PCIe Endpoint",
            1 => $"usb {random.Next(1, 5)}-{random.Next(1, 9)}: new high-speed USB device number {random.Next(2, 12)} using xhci_hcd",
            2 => $"EXT4-fs (nvme0n1p{random.Next(1, 4)}): mounted filesystem {random.Uuid()} r/w with ordered data mode. Quota mode: none.",
            3 => $"e1000e 0000:00:1f.6 eth0: (PCI Express:2.5GT/s:Width x1) {random.MacAddress()}",
            4 => $"nvme nvme0: {random.Next(4, 33)}/0/0 default/read/poll queues",
            5 => $"audit: type=1400 audit({random.Next(1700000000, 1760000000)}.{random.Next(100, 999)}:{random.Next(2, 400)}): apparmor=\"STATUS\" operation=\"profile_load\" profile=\"unconfined\" name=\"{random.Pick(["/usr/sbin/nginx", "/usr/bin/man", "lsb_release", "nvidia_modprobe", "/usr/sbin/chronyd", "docker-default"])}\" pid={random.Next(300, 2000)} comm=\"apparmor_parser\"",
            6 => $"input: {random.Pick(["AT Translated Set 2 keyboard", "Power Button", "Logitech USB Receiver", "Video Bus", "PC Speaker"])} as /devices/platform/i8042/serio0/input/input{random.Next(0, 24)}",
            7 => $"NET: Registered PF_{random.Pick(["INET6", "PACKET", "XDP", "ALG", "VSOCK", "NETLINK", "UNIX"])} protocol family",
            8 => $"x86/fpu: Supporting XSAVE feature 0x{random.Next(1, 512):x3}: '{random.Pick(CpuFeatures)}'",
            9 => $"smpboot: CPU{random.Next(1, 32)} is up",
            10 => $"intel_rapl_common: Found RAPL domain {random.Pick(["package", "core", "uncore", "dram", "psys"])}",
            11 => $"zram0: detected capacity change from 0 to {random.Next(8, 64) * 1048576}",
            12 => $"{random.Pick(KernelModules)}: module verification failed: signature and/or required key missing - tainting kernel",
            13 => $"br-{random.Hex(12)}: port {random.Next(1, 6)}(veth{random.Hex(7)}) entered {random.Pick(["blocking", "forwarding", "disabled"])} state",
            14 => $"eth0: renamed from veth{random.Hex(7)}",
            15 => $"IPv6: ADDRCONF(NETDEV_CHANGE): veth{random.Hex(7)}: link becomes ready",
            16 => $"systemd[1]: Started {random.Pick(SystemdServices)}.",
            17 => $"clocksource: tsc: mask: 0xffffffffffffffff max_cycles: 0x{random.Hex(11)}, max_idle_ns: {random.Next(100000000, 900000000)} ns",
            18 => $"ACPI: \\_SB_.PCI0.GP{random.Next(10, 30)}: PCI Interrupt Link [LNK{(char)('A' + random.Next(0, 8))}] enabled at IRQ {random.Next(16, 40)}",
            19 => $"Memory: {random.Next(15800000, 16300000)}K/{random.Next(16600000, 16800000)}K available ({random.Next(18000, 22000)}K kernel code, {random.Next(4000, 5000)}K rwdata, {random.Next(12000, 14000)}K rodata)",
            20 => $"TCP: {random.Pick(["cubic", "bbr", "reno"])} registered",
            _ => $"md/raid1:md{random.Next(0, 3)}: active with 2 out of 2 mirrors",
        };
    }

    /// <summary>Formats seconds since boot the way dmesg does: "[   12.345678]".</summary>
    public static string KernelTimestamp(double secondsSinceBoot)
    {
        return $"[{secondsSinceBoot.ToString("F6", System.Globalization.CultureInfo.InvariantCulture),12}]";
    }

    /// <summary>Formats a byte count like "12.4 MiB" or "512 KiB".</summary>
    public static string HumanReadableSize(double bytes)
    {
        string[] units = ["B", "KiB", "MiB", "GiB"];
        int unitIndex = 0;
        while (bytes >= 1024 && unitIndex < units.Length - 1)
        {
            bytes /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{bytes:0} {units[unitIndex]}"
            : $"{bytes.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} {units[unitIndex]}";
    }
}
