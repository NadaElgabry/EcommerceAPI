using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Settings
{
    public class AwsSettings
    {
        public string Region { get; set; } = default!;
        public S3Settings S3 { get; set; } = default!;
    }

    public class S3Settings
    {
        public string BucketName { get; set; } = default!;
    }
}
