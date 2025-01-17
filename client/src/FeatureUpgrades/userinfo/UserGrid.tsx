import * as React from 'react';
import { Grid, GridColumn as Column, GridEvent } from '@progress/kendo-react-grid';
import { UseAppDispatch, useAppSelector } from '../../app/store/ConfigureStore';
import { useDispatch } from 'react-redux';
import { fetchProductsAsync, productSelectors } from '../../features/catalog/CatalogSlice';
import { Product } from '../../app/models/Product';
import LoadingComponent from '../../app/layouts/LoadingComponent';

// const availableProducts = products.slice();

// const initialProducts: Array<any> = availableProducts.splice(0, 20);

const UserGrid = () => {
    const dispatch = UseAppDispatch();
    const productLoaded = useAppSelector(x=>x.catalog.productLoaded);
    const products = useAppSelector(productSelectors.selectAll);
    const availableProducts = products;
    const initialProducts = availableProducts.splice(0,20);
    React.useEffect(()=>
    {
        if(!productLoaded)
        {
            dispatch(fetchProductsAsync);   
        }
    },[productLoaded,dispatch]);
    // const [gridData, setGridData] = React.useState<Product[]>(initialProducts);

    const [gridData,setGridData] = React.useState<Product[]>(initialProducts);
    const scrollHandler = (event: GridEvent) => {
        const e = event.nativeEvent;
        if (e.target.scrollTop + 10 >= e.target.scrollHeight - e.target.clientHeight) {
            const moreData = availableProducts.splice(0, 10);
            if (moreData.length > 0) {
                setGridData((oldData) => oldData.concat(moreData));
            }
        }
    };

    return (
        <div>
            <Grid
                style={{ height: '400px' }}
                data={gridData}
                onScroll={scrollHandler}
                fixedScroll={true}
                rowHeight={50}
            >
                <Column field="id" title="ID" width="40px" />
                <Column field="name" title="Name" width="250px" />
                <Column field="price" width="250px" />
                <Column field="pictureUrl" width="250px" />
                <Column field="type" width="250px" />
                <Column field="brand" width="250px" />
                <Column field="quantityInStock" width="250px" />
            </Grid>
            <br />
            showing: {gridData.length} items
        </div>
    );
};

export default UserGrid;
