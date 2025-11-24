using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;

namespace ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KafkaMonitoringController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaMonitoringController> _logger;

    public KafkaMonitoringController(
        IConfiguration configuration,
        ILogger<KafkaMonitoringController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Verifica a saúde do Kafka
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetKafkaHealth()
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            return Ok(new
            {
                status = "Healthy",
                bootstrapServers = bootstrapServers,
                brokers = metadata.Brokers.Count,
                topics = metadata.Topics.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Kafka health");
            return StatusCode(500, new
            {
                status = "Unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Lista todos os tópicos do Kafka
    /// </summary>
    [HttpGet("topics")]
    public IActionResult GetTopics()
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            var topics = metadata.Topics
                .Where(t => !t.Topic.StartsWith("__")) // Filtrar tópicos internos
                .Select(t => new
                {
                    name = t.Topic,
                    partitions = t.Partitions.Count,
                    partitionDetails = t.Partitions.Select(p => new
                    {
                        id = p.PartitionId,
                        leader = p.Leader,
                        replicas = p.Replicas.Length,
                        isr = p.InSyncReplicas.Length
                    })
                });

            return Ok(topics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Kafka topics");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém informações de um tópico específico
    /// </summary>
    [HttpGet("topics/{topicName}")]
    public IActionResult GetTopicInfo(string topicName)
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromSeconds(5));

            var topic = metadata.Topics.FirstOrDefault(t => t.Topic == topicName);

            if (topic == null)
            {
                return NotFound(new { error = $"Topic '{topicName}' not found" });
            }

            return Ok(new
            {
                name = topic.Topic,
                partitions = topic.Partitions.Count,
                partitionDetails = topic.Partitions.Select(p => new
                {
                    id = p.PartitionId,
                    leader = p.Leader,
                    replicas = p.Replicas.Length,
                    inSyncReplicas = p.InSyncReplicas.Length,
                    error = p.Error.Code != ErrorCode.NoError ? p.Error.Reason : null
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting topic info for {TopicName}", topicName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lista todos os consumer groups
    /// </summary>
    [HttpGet("consumer-groups")]
    public IActionResult GetConsumerGroups()
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var groupsResult = adminClient.ListGroups(TimeSpan.FromSeconds(10));

            // Correção: usar List ao invés de Groups
            var result = groupsResult.ToList().Select(g => new
            {
                groupId = g.Group,
                protocol = g.ProtocolType,
                state = g.State
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing consumer groups");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém informações detalhadas de um consumer group
    /// </summary>
    [HttpGet("consumer-groups/{groupId}")]
    public IActionResult GetConsumerGroupInfo(string groupId)
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var groupsResult = adminClient.ListGroups(TimeSpan.FromSeconds(10));

            // Correção: usar List ao invés de Groups
            var group = groupsResult.ToList().FirstOrDefault(g => g.Group == groupId);

            if (group == null)
            {
                return NotFound(new { error = $"Consumer group '{groupId}' not found" });
            }

            return Ok(new
            {
                groupId = group.Group,
                protocol = group.ProtocolType,
                state = group.State,
                members = group.Members.Select(m => new
                {
                    memberId = m.MemberId,
                    clientId = m.ClientId,
                    host = m.ClientHost
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer group info for {GroupId}", groupId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém offsets dos consumer groups
    /// </summary>
    [HttpGet("consumer-groups/{groupId}/offsets")]
    public IActionResult GetConsumerGroupOffsets(string groupId)
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            var allOffsets = new List<object>();

            foreach (var topic in metadata.Topics)
            {
                if (topic.Topic.StartsWith("__"))
                    continue;

                var topicPartitions = topic.Partitions.Select(p =>
                    new TopicPartition(topic.Topic, new Partition(p.PartitionId))
                ).ToList();

                try
                {
                    var committed = consumer.Committed(topicPartitions, TimeSpan.FromSeconds(5));

                    var partitionOffsets = committed
                        .Where(tp => tp.Offset != Offset.Unset)
                        .Select(tp => new
                        {
                            partition = tp.Partition.Value,
                            offset = tp.Offset.Value
                        })
                        .ToList();

                    if (partitionOffsets.Any())
                    {
                        allOffsets.Add(new
                        {
                            topic = topic.Topic,
                            partitions = partitionOffsets
                        });
                    }
                }
                catch (KafkaException ex)
                {
                    _logger.LogWarning(ex, "Could not get offsets for topic {Topic}", topic.Topic);
                }
            }

            return Ok(new
            {
                groupId,
                topics = allOffsets
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer group offsets for {GroupId}", groupId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém lag dos consumer groups
    /// </summary>
    [HttpGet("consumer-groups/{groupId}/lag")]
    public IActionResult GetConsumerGroupLag(string groupId)
    {
        try
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            var lagInfo = new List<object>();

            foreach (var topic in metadata.Topics)
            {
                if (topic.Topic.StartsWith("__"))
                    continue;

                var topicPartitions = topic.Partitions.Select(p =>
                    new TopicPartition(topic.Topic, new Partition(p.PartitionId))
                ).ToList();

                try
                {
                    var committed = consumer.Committed(topicPartitions, TimeSpan.FromSeconds(5));
                    var watermarkOffsets = topicPartitions.Select(tp =>
                    {
                        var watermark = consumer.QueryWatermarkOffsets(tp, TimeSpan.FromSeconds(5));
                        return new { tp, watermark };
                    }).ToList();

                    var partitionLags = committed
                        .Where(tp => tp.Offset != Offset.Unset)
                        .Select(committedOffset =>
                        {
                            var watermark = watermarkOffsets
                                .FirstOrDefault(w => w.tp.Partition == committedOffset.Partition);

                            if (watermark != null)
                            {
                                var lag = watermark.watermark.High.Value - committedOffset.Offset.Value;
                                return new
                                {
                                    partition = committedOffset.Partition.Value,
                                    currentOffset = committedOffset.Offset.Value,
                                    highWatermark = watermark.watermark.High.Value,
                                    lag = lag < 0 ? 0 : lag
                                };
                            }
                            return null;
                        })
                        .Where(x => x != null)
                        .ToList();

                    if (partitionLags.Any())
                    {
                        lagInfo.Add(new
                        {
                            topic = topic.Topic,
                            partitions = partitionLags,
                            totalLag = partitionLags.Sum(p => p?.lag ?? 0)
                        });
                    }
                }
                catch (KafkaException ex)
                {
                    _logger.LogWarning(ex, "Could not get lag for topic {Topic}", topic.Topic);
                }
            }

            return Ok(new
            {
                groupId,
                topics = lagInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer group lag for {GroupId}", groupId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}