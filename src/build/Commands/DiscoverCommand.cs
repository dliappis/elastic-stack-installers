﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ElastiBuild;
using ElastiBuild.Options;
using static Bullseye.Targets;

namespace ElastiBuild.Commands
{
    [Verb("discover", HelpText = "Discover Container Id's used for build and fetch commands")]
    public class DiscoverCommand
        : IElastiBuildCommand
        , ISupportTargets
        , ISupportContainerId
        , ISupportOssChoice
        , ISupportBitnessChoice
    {
        public IEnumerable<string> Targets { get; set; } = new List<string>();
        public string ContainerId { get; set; }
        public bool ShowOss { get; set; }
        public eBitness Bitness { get; set; }

        public async Task RunAsync(BuildContext ctx_)
        {
            if (Targets.Any(t => t.ToLower() == "all"))
            {
                var items = await ArtifactsApi.ListNamedContainers();

                await Console.Out.WriteLineAsync(Environment.NewLine + "Branches:");
                await Console.Out.WriteLineAsync(string.Join(
                    Environment.NewLine,
                    items
                        .Where(itm => itm.IsBranch)
                        .Select(itm => "  " + itm.Name)
                    ));

                await Console.Out.WriteLineAsync(Environment.NewLine + "Versions:");
                await Console.Out.WriteLineAsync(string.Join(
                    Environment.NewLine,
                    items
                        .Where(itm => itm.IsVersion)
                        .Select(itm => "  " + itm.Name)
                    ));

                await Console.Out.WriteLineAsync(Environment.NewLine + "Aliases:");
                await Console.Out.WriteLineAsync(string.Join(
                    Environment.NewLine,
                    items
                        .Where(itm => itm.IsAlias)
                        .Select(itm => "  " + itm.Name)
                    ));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ContainerId) || ContainerId.ToLower() == "all")
                {
                    await Console.Out.WriteLineAsync(
                        $"ERROR(s):{Environment.NewLine}" +
                        $"  Need --arid when TARGET specified. Run with --help for more information.");
                    return;
                }

                foreach (var target in Targets)
                {
                    await Console.Out.WriteLineAsync(Environment.NewLine +
                        $"Discovering '{target}' in '{ContainerId}' ...");

                    var items = await ArtifactsApi.FindArtifact(target, filter =>
                    {
                        filter.ContainerId = ContainerId;
                        filter.ShowOss = ShowOss;
                        filter.Bitness = Bitness;
                    });

                    await Console.Out.WriteLineAsync(string.Join(
                        Environment.NewLine,
                        items
                            .Select(itm => "  " + itm.FileName)
                        ));
                }
            }
        }

        [Usage(ApplicationAlias = GlobalOptions.AppAlias)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example(Environment.NewLine +
                        "Discover branches, versions and aliases",
                        new DiscoverCommand()
                        {
                            Targets = new List<string> { "all" },
                        }),

                    new Example(Environment.NewLine +
                        "Show available Winlogbeat packages for alias 6.8",
                        new DiscoverCommand
                        {
                            ContainerId = "6.8",
                            Targets = new List<string> { "winlogbeat" },
                        })
                };
            }
        }

    }
}
