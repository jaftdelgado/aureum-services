import { Test, TestingModule } from '@nestjs/testing';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ConfigModule } from '@nestjs/config';
import { AssetService } from '../src/services/asset.service';
import { Asset } from '../src/entities/asset.entity';
import { AssetCategory } from '../src/entities/assetCategory.entity';
import { TeamAsset } from '../src/entities/teamAsset.entity'; 
import { Movement } from '../src/entities/movement.entity';   

const mockMarketDbParams = {
  type: 'postgres' as const,
  database: 'test_market_db', 
  entities: [TeamAsset, Movement],
  synchronize: true,
  dropSchema: true,  
};

describe('AssetService - Integration Tests', () => {
  let service: AssetService;
  let moduleRef: TestingModule;

  beforeAll(async () => {
    moduleRef = await Test.createTestingModule({
      imports: [
        ConfigModule.forRoot({ isGlobal: true }),
        TypeOrmModule.forRoot({
          name: 'assetsConnection',
          type: 'postgres',
          url: process.env.ASSETS_DB_URL || 'postgres://test_user:test_pass@localhost:5432/test_assets_db',
          entities: [Asset, AssetCategory],
          synchronize: true,
          dropSchema: true,
          ssl: false,
        }),
        TypeOrmModule.forRoot({
          name: 'marketConnection',
          type: 'postgres',
          url: process.env.MARKET_DB_URL || 'postgres://test_user:test_pass@localhost:5432/test_assets_db', // Reutilizamos la misma DB para el test
          entities: [TeamAsset, Movement],
          synchronize: true, 
          ssl: false,
        }),
        TypeOrmModule.forFeature([Asset, AssetCategory], 'assetsConnection'),
      ],
      providers: [AssetService],
    }).compile();

    service = moduleRef.get<AssetService>(AssetService);
  });

  afterAll(async () => {
    await moduleRef.close();
  });

  test('Debe estar definido', () => {
    expect(service).toBeDefined();
  });

  test('Debe crear datos semilla y recuperar assets paginados', async () => {
    const categoryRepo = moduleRef.get('AssetCategoryRepository');
    const connection = moduleRef.get('TypeOrmModuleOptions_assetsConnection_Connection'); 
    
    const dataSource = moduleRef.get('DataSource_assetsConnection');
    const catRepo = dataSource.getRepository(AssetCategory);
    const assetRepo = dataSource.getRepository(Asset);

    const category = await catRepo.save({
        categoryKey: 'stocks',
    });

    await assetRepo.save({
        assetSymbol: 'AAPL',
        assetName: 'Apple Inc.',
        assetType: 'Stock',
        basePrice: 150.0,
        category: category
    });

    const result = await service.getAssets({ page: 1, limit: 10 });
    
    expect(result.data).toHaveLength(1);
    expect(result.data[0].assetSymbol).toBe('AAPL');
    expect(result.meta.totalItems).toBe(1);
  });

  test('Debe encontrar un asset por su Public ID', async () => {
     const dataSource = moduleRef.get('DataSource_assetsConnection');
     const assetRepo = dataSource.getRepository(Asset);

     const asset = await assetRepo.save({
        assetSymbol: 'GOOGL',
        assetName: 'Alphabet',
        assetType: 'Stock',
        basePrice: 200.0,
     });

     const found = await service.findOneByPublicId(asset.publicId);
     expect(found.assetName).toBe('Alphabet');
  });
});