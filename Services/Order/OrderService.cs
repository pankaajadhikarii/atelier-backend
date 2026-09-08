using System.Text.Json;
using Atelier_backend.Data;
using Atelier_backend.Models.DTOs.Order;
using Atelier_backend.Models.DTOs.Payment;
using Atelier_backend.Models.DTOs.Review;
using Atelier_backend.Models.Entities;
using Atelier_backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Atelier_backend.Services;

public class OrderService : IOrderService
{
    private const decimal CustomizationPrice = 0m;
    private const decimal DeliveryFee = 100m;

    private readonly ApplicationDbContext _dbContext;

    public OrderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetAllAsync(
        string customerId)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Garment)
            .Include(order => order.Design)
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return orders
            .Select(order => new OrderSummaryDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                GarmentName = order.Garment.Name,
                DesignName = order.Design.Name,
                DesignImageUrl = order.Design.ImageUrl,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                OrderDate = order.OrderDate
            })
            .ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(
        int id,
        string customerId)
    {
        var order = await GetOrderQuery()
            .Where(order =>
                order.Id == id &&
                order.CustomerId == customerId)
            .SingleOrDefaultAsync();

        return order == null
            ? null
            : MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(
        CreateOrderDto createDto,
        string customerId)
    {
        ValidateEnum(
            createDto.FitType,
            "Fit type");

        ValidateEnum(
            createDto.PaymentMethod,
            "Payment method");

        if (createDto.FabricQuantity <= 0)
        {
            throw new InvalidOperationException(
                "Fabric quantity must be greater than zero.");
        }

        var shippingAddress =
            createDto.ShippingAddress.Trim();

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            throw new InvalidOperationException(
                "Shipping address is required.");
        }

        var customizationDetailsJson =
            NormalizeJson(
                createDto.CustomizationDetailsJson);

        if (customizationDetailsJson != null)
        {
            ValidateJsonObject(
                customizationDetailsJson,
                "Customization details");
        }

        var customer = await _dbContext.Users
            .SingleOrDefaultAsync(user =>
                user.Id == customerId &&
                user.IsActive);

        if (customer == null)
        {
            throw new KeyNotFoundException(
                "Customer account not found.");
        }

        var fabric = await _dbContext.Fabrics
            .SingleOrDefaultAsync(f =>
                f.Id == createDto.FabricId &&
                f.IsActive);

        if (fabric == null)
        {
            throw new KeyNotFoundException(
                "The selected fabric does not exist or is inactive.");
        }

        if (createDto.FabricQuantity >
            fabric.AvailableQuantity)
        {
            throw new InvalidOperationException(
                "The requested fabric quantity is not available.");
        }

        var garment = await _dbContext.Garments
            .SingleOrDefaultAsync(g =>
                g.Id == createDto.GarmentId &&
                g.IsActive);

        if (garment == null)
        {
            throw new KeyNotFoundException(
                "The selected garment does not exist or is inactive.");
        }

        var design = await _dbContext.Designs
            .SingleOrDefaultAsync(d =>
                d.Id == createDto.DesignId &&
                d.IsActive);

        if (design == null)
        {
            throw new KeyNotFoundException(
                "The selected design does not exist or is inactive.");
        }

        if (design.GarmentId != garment.Id)
        {
            throw new InvalidOperationException(
                "The selected design does not belong to the selected garment.");
        }

        var measurementProfile =
            await _dbContext.MeasurementProfiles
                .SingleOrDefaultAsync(profile =>
                    profile.Id == createDto.MeasurementProfileId &&
                    profile.UserId == customerId);

        if (measurementProfile == null)
        {
            throw new KeyNotFoundException(
                "The selected measurement profile was not found.");
        }

        if (measurementProfile.GarmentId != garment.Id)
        {
            throw new InvalidOperationException(
                "The selected measurement profile does not belong to the selected garment.");
        }

        var measurementSnapshotJson =
            measurementProfile.MeasurementValuesJson.Trim();

        ValidateJsonObject(
            measurementSnapshotJson,
            "Measurement profile values");

        var now = DateTime.UtcNow;

        var fabricPrice =
            fabric.Price * createDto.FabricQuantity;

        var tailoringPrice =
            (garment.BasePrice ?? 0m) +
            design.TailoringPrice;

        var totalAmount =
            fabricPrice +
            tailoringPrice +
            CustomizationPrice +
            DeliveryFee;

        var order = new Order
        {
            OrderNumber =
                await GenerateOrderNumberAsync(now),

            CustomerId =
                customer.Id,

            FabricId =
                fabric.Id,

            GarmentId =
                garment.Id,

            DesignId =
                design.Id,

            MeasurementProfileId =
                measurementProfile.Id,

            MeasurementSnapshotJson =
                measurementSnapshotJson,

            FitType =
                createDto.FitType,

            CustomizationDetailsJson =
                customizationDetailsJson,

            SpecialInstructions =
                NormalizeOptional(
                    createDto.SpecialInstructions),

            FabricQuantity =
                createDto.FabricQuantity,

            FabricPrice =
                fabricPrice,

            TailoringPrice =
                tailoringPrice,

            CustomizationPrice =
                CustomizationPrice,

            DeliveryFee =
                DeliveryFee,

            TotalAmount =
                totalAmount,

            Status =
                OrderStatus.Pending,

            PaymentStatus =
                OrderPaymentStatus.Unpaid,

            PaymentMethod =
                createDto.PaymentMethod,

            ShippingAddress =
                shippingAddress,

            OrderDate =
                now,

            UpdatedAt =
                now
        };

        _dbContext.Orders.Add(order);

        await _dbContext.SaveChangesAsync();

        var createdOrder = await GetOrderQuery()
            .SingleAsync(created =>
                created.Id == order.Id &&
                created.CustomerId == customerId);

        return MapToDto(createdOrder);
    }

    public async Task<OrderDto?> CancelAsync(
        int id,
        string customerId)
    {
        var order = await _dbContext.Orders
            .SingleOrDefaultAsync(order =>
                order.Id == id &&
                order.CustomerId == customerId);

        if (order == null)
        {
            return null;
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending orders can be cancelled.");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var cancelledOrder = await GetOrderQuery()
            .SingleAsync(order =>
                order.Id == id &&
                order.CustomerId == customerId);

        return MapToDto(cancelledOrder);
    }

    private IQueryable<Order> GetOrderQuery()
    {
        return _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Fabric)
            .Include(order => order.Garment)
            .Include(order => order.Design)
            .Include(order => order.MeasurementProfile)
            .Include(order => order.Payment)
            .Include(order => order.Review)
                .ThenInclude(review => review!.Customer);
    }

    private async Task<string> GenerateOrderNumberAsync(
        DateTime orderDate)
    {
        string orderNumber;

        do
        {
            orderNumber =
                $"ATL-{orderDate:yyyyMMdd}-{Guid.NewGuid():N}"
                    .ToUpperInvariant();
        }
        while (await _dbContext.Orders
            .AnyAsync(order =>
                order.OrderNumber == orderNumber));

        return orderNumber;
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,

            CustomerId =
                order.CustomerId,

            CustomerName =
                order.Customer.FullName,

            CustomerEmail =
                order.Customer.Email ?? string.Empty,

            CustomerPhone =
                order.Customer.PhoneNumber,

            FabricId =
                order.FabricId,

            FabricName =
                order.Fabric.Name,

            FabricImageUrl =
                order.Fabric.ImageUrl,

            GarmentId =
                order.GarmentId,

            GarmentName =
                order.Garment.Name,

            DesignId =
                order.DesignId,

            DesignName =
                order.Design.Name,

            DesignImageUrl =
                order.Design.ImageUrl,

            MeasurementProfileId =
                order.MeasurementProfileId,

            MeasurementSnapshotJson =
                order.MeasurementSnapshotJson,

            FitType =
                order.FitType,

            CustomizationDetailsJson =
                order.CustomizationDetailsJson,

            SpecialInstructions =
                order.SpecialInstructions,

            FabricQuantity =
                order.FabricQuantity,

            FabricPrice =
                order.FabricPrice,

            TailoringPrice =
                order.TailoringPrice,

            CustomizationPrice =
                order.CustomizationPrice,

            DeliveryFee =
                order.DeliveryFee,

            TotalAmount =
                order.TotalAmount,

            Status =
                order.Status,

            RejectionReason =
                order.RejectionReason,

            PaymentStatus =
                order.PaymentStatus,

            PaymentMethod =
                order.PaymentMethod,

            ShippingAddress =
                order.ShippingAddress,

            OrderDate =
                order.OrderDate,

            UpdatedAt =
                order.UpdatedAt,

            Payment = order.Payment == null
                ? null
                : new PaymentDto
                {
                    Id =
                        order.Payment.Id,

                    OrderId =
                        order.Payment.OrderId,

                    OrderNumber =
                        order.OrderNumber,

                    CustomerId =
                        order.Payment.CustomerId,

                    CustomerName =
                        order.Customer.FullName,

                    TransactionReference =
                        order.Payment.TransactionReference,

                    PaymentMethod =
                        order.Payment.PaymentMethod,

                    Amount =
                        order.Payment.Amount,

                    Status =
                        order.Payment.Status,

                    PaidAt =
                        order.Payment.PaidAt,

                    CreatedAt =
                        order.Payment.CreatedAt
                },

            Review = order.Review == null
                ? null
                : new ReviewDto
                {
                    Id =
                        order.Review.Id,

                    OrderId =
                        order.Review.OrderId,

                    CustomerId =
                        order.Review.CustomerId,

                    CustomerName =
                        order.Review.Customer.FullName,

                    Rating =
                        order.Review.Rating,

                    Comment =
                        order.Review.Comment,

                    AdminResponse =
                        order.Review.AdminResponse,

                    CreatedAt =
                        order.Review.CreatedAt
                }
        };
    }

    private static void ValidateEnum<TEnum>(
        TEnum value,
        string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new InvalidOperationException(
                $"{fieldName} is invalid.");
        }
    }

    private static void ValidateJsonObject(
        string value,
        string fieldName)
    {
        try
        {
            using var document =
                JsonDocument.Parse(value);

            if (document.RootElement.ValueKind !=
                JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    $"{fieldName} must be a JSON object.");
            }
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{fieldName} must contain valid JSON.");
        }
    }

    private static string? NormalizeJson(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}