package db

import (
	"time"

	"github.com/google/uuid"
)

// ========== movements ==========

type Movement struct {
	MovementID  int       `gorm:"column:movementid;primaryKey;autoIncrement"`
	PublicID    uuid.UUID `gorm:"column:publicid;type:uuid;default:uuid_generate_v4()"`
	UserID      uuid.UUID `gorm:"column:userid;type:uuid;not null"`
	AssetID     uuid.UUID `gorm:"column:assetid;type:uuid;not null"`
	Quantity    float64   `gorm:"column:quantity"`    // DECIMAL(18,6)
	CreatedDate time.Time `gorm:"column:createddate"` // TIMESTAMP
}

func (Movement) TableName() string { return "movements" }

// ========== transactions ==========

type Transaction struct {
	TransactionID    int       `gorm:"column:transactionid;primaryKey;autoIncrement"`
	PublicID         uuid.UUID `gorm:"column:publicid;type:uuid;default:uuid_generate_v4()"`
	MovementID       int       `gorm:"column:movementid;not null;unique"`
	TransactionPrice float64   `gorm:"column:transactionprice"` // DECIMAL(18,6)
	IsBuy            bool      `gorm:"column:isbuy"`
	CreatedDate      time.Time `gorm:"column:createddate"`

	// Relación con Movement (opcional)
	Movement Movement `gorm:"foreignKey:MovementID;references:MovementID"`
}

func (Transaction) TableName() string { return "transactions" }

// ========== historicalprices ==========

type HistoricalPrice struct {
	HistoricalPriceID int       `gorm:"column:historicalpriceid;primaryKey;autoIncrement"`
	PublicID          uuid.UUID `gorm:"column:publicid;type:uuid;default:uuid_generate_v4()"`
	Price             float64   `gorm:"column:price"`
	TeamAssetID       int       `gorm:"column:teamassetid"`
}

func (HistoricalPrice) TableName() string { return "historicalprices" }

// ========== marketconfigurations ==========

type MarketConfiguration struct {
	ConfigID    int       `gorm:"column:configid;primaryKey;autoIncrement"`
	PublicID    uuid.UUID `gorm:"column:publicid;type:uuid;default:uuid_generate_v4()"`
	TeamID      int       `gorm:"column:teamid;unique;not null"`
	InitialCash float64   `gorm:"column:initialcash"`

	Currency         string `gorm:"column:currency;type:currency_enum"`
	MarketVolatility string `gorm:"column:marketvolatility;type:volatility_enum"`
	MarketLiquidity  string `gorm:"column:marketliquidity;type:volatility_enum"`
	ThickSpeed       string `gorm:"column:thickspeed;type:thick_speed_enum"`

	TransactionFee string `gorm:"column:transactionfee;type:transaction_fee_enum"`
	EventFrequency string `gorm:"column:eventfrequency;type:transaction_fee_enum"`
	DividendImpact string `gorm:"column:dividendimpact;type:transaction_fee_enum"`
	CrashImpact    string `gorm:"column:crashimpact;type:transaction_fee_enum"`

	AllowShortSelling bool `gorm:"column:allowshortselling"`

	CreatedAt time.Time `gorm:"column:createdat"`
	UpdatedAt time.Time `gorm:"column:updatedat"`
}

func (MarketConfiguration) TableName() string { return "marketconfigurations" }

// ========== teamassets ==========

type TeamAsset struct {
	TeamAssetID  int       `gorm:"column:teamassetid;primaryKey;autoIncrement"`
	PublicID     uuid.UUID `gorm:"column:publicid;type:uuid;default:uuid_generate_v4()"`
	TeamID       int       `gorm:"column:teamid;not null"`
	AssetID      int       `gorm:"column:assetid;not null"`
	CurrentPrice float64   `gorm:"column:currentprice"`
}

func (TeamAsset) TableName() string { return "teamassets" }
