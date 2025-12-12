import { Test, TestingModule } from '@nestjs/testing';
import { TypeOrmModule, getDataSourceToken } from '@nestjs/typeorm';
import { ConfigModule } from '@nestjs/config';
import { DataSource } from 'typeorm';
import { AssetService } from '../src/services/asset.service';
import { Asset } from '../src/entities/asset.entity';
import { AssetCategory } from '../src/entities/assetCategory.entity';
import { TeamAsset } from '../src/entities/teamAsset.entity';
import { Movement } from '../src/entities/movement.entity';

jest.setTimeout(30000);

describe('AssetService - Integration Tests', () => {
  let service: AssetService;
  let moduleRef: TestingModule;
  let assetsDataSource: DataSource;

  beforeAll(async () => {
    moduleRef = await Test.createTestingModule({
      imports: [
        ConfigModule.forRoot({ isGlobal: true }),
        TypeOrmModule.forRoot({
          name: 'assetsConnection',
          type: 'postgres',
          url: process.env.ASSETS_DB_URL || 'postgres://test_user:test_pass@localhost:5432/test_assets_db',
          entities: [Asset, AssetCategory],
          synchronize: false, 
          dropSchema: true,
          ssl: false,
        }),
        TypeOrmModule.forRoot({
          name: 'marketConnection',
          type: 'postgres',
          url: process.env.MARKET_DB_URL || 'postgres://test_user:test_pass@localhost:5432/test_assets_db',
          entities: [TeamAsset, Movement],
          synchronize: false,
          ssl: false,
        }),
        TypeOrmModule.forFeature([Asset, AssetCategory], 'assetsConnection'),
      ],
      providers: [AssetService],
    }).compile();

    service = moduleRef.get<AssetService>(AssetService);

    assetsDataSource = moduleRef.get<DataSource>(getDataSourceToken('assetsConnection'));

    await assetsDataSource.query('CREATE EXTENSION IF NOT EXISTS "uuid-ossp";');

    await assetsDataSource.synchronize();
    
    const marketDataSource = moduleRef.get<DataSource>(getDataSourceToken('marketConnection'));
    if (marketDataSource && marketDataSource !== assetsDataSource) {
        await marketDataSource.synchronize();
    }
  });

  afterAll(async () => {
    if (moduleRef) {
      await moduleRef.close();
    }
  });

  test('Debe estar definido', () => {
    expect(service).toBeDefined();
  });

  test('Debe crear datos semilla y recuperar assets paginados', async () => {
    const catRepo = assetsDataSource.getRepository(AssetCategory);
    const assetRepo = assetsDataSource.getRepository(Asset);

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
     const assetRepo = assetsDataSource.getRepository(Asset);

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
